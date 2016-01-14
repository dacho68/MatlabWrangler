using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//some that I added
using System.IO;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MatlabWrangler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        //lists to keep track of
        private BindingList<string> models = new BindingList<string>();
        private BindingList<string> filepaths = new BindingList<string>();
        BackgroundWorker numberCruncher;
        private bool numberCrunchingInProgress;
        #endregion

        #region UI functions
        public MainWindow()
        {
            InitializeComponent();
            
            //link up to those bindings
            modelBox.ItemsSource = models;
            outputBox.ItemsSource = filepaths;

            //numberCrunchingInProgress needs to be initialized to false so we can use the UI, it will lock kind of while it is crunching
            numberCrunchingInProgress = false;
        }

        private void generate_button_Click(object sender, RoutedEventArgs e)
        {
            if (numberCrunchingInProgress == false)
            {
                //lock down the UI (kind of... it isn't frozen, but now just returns message boxes)
                numberCrunchingInProgress = true;

                //generating background process message
                myTextBox.Text = "generating the background thread to crunch the numbers";

                //initialize the background process
                numberCruncher = new BackgroundWorker();
                //reports the progress for our progress bar
                numberCruncher.WorkerReportsProgress = true;
                numberCruncher.DoWork += new DoWorkEventHandler(numberCruncher_DoWork);
                numberCruncher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(numberCruncher_RunWorkerCompleted);
                numberCruncher.ProgressChanged += new ProgressChangedEventHandler(numberCruncher_ReportProgress);
                
                //starts the asyncronous computation, leaving your UI free to be operated on
                numberCruncher.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("You cannot start the generation process while it is already running! Patience, young padawan...");
            }
        }
        
        private void choose_folder_click(object sender, RoutedEventArgs e)
        {
            if (numberCrunchingInProgress == false)
            {
                CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
                openFileDialog.Title = "Destination Folder for .m file";
                openFileDialog.IsFolderPicker = true;
                //openFileDialog.InitialDirectory = ???

                openFileDialog.AddToMostRecentlyUsedList = false;
                openFileDialog.AllowNonFileSystemItems = false;
                //openFileDialog.DefaultDirectory = ???
                openFileDialog.EnsureFileExists = true;
                openFileDialog.EnsurePathExists = true;
                openFileDialog.EnsureReadOnly = false;
                openFileDialog.EnsureValidNames = true;
                openFileDialog.Multiselect = false;
                openFileDialog.ShowPlacesList = true;

                //did it work?
                if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string folder = openFileDialog.FileName;
                    //refresh folder list...
                    string filePath = folder + "\\defineVehicleBody.m";
                    filepaths.Add(filePath);
                    outputBox.ItemsSource = null;
                    outputBox.ItemsSource = filepaths;

                    //now tell the user to push generate
                    myTextBox.Text = "after dragging a .stl file into the top box, push generate";
                }
            }
            else
            {
                MessageBox.Show("You can't swap folders while the .m file is being generated. Patience, young padawan...");
            }
        }

        private void modelBox_DragEnter(object sender, DragEventArgs e)
        {
            if (numberCrunchingInProgress == false)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effects = DragDropEffects.Copy;
            }
            else
            {
                MessageBox.Show("Stop doing stuff while it is working!");
            }
        }

        private void modelBox_Drop(object sender, DragEventArgs e)
        {
            if (numberCrunchingInProgress == false)
            {
                string[] modelPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string modelPath in modelPaths)
                {
                    //make sure that the path is a .stl path
                    string extension = modelPath.Split('.').Last();
                    if (extension.Equals("stl"))
                    {
                        if (models.Count > 0)
                            models.Clear();
                        models.Add(modelPath);
                    }
                    else
                        MessageBox.Show("Only .stl files are allowed!");
                }
                //refresh listbox
                modelBox.ItemsSource = null;
                modelBox.ItemsSource = models;
            }
            else
            {
                MessageBox.Show("Stop doing stuff while it is working!");
            }
        }
        #endregion

        #region background processes
        private void numberCruncher_DoWork(object sender, DoWorkEventArgs e)
        {
            numberCruncher.ReportProgress(0, "Opening Files...");

            //open files to operate on (the .m file and the .stl)
            TextWriter mFile = new StreamWriter(filepaths.FirstOrDefault());
            StreamReader stlFile = new StreamReader(models.FirstOrDefault());
            List<List<double>> vertexList = new List<List<double>>();
            List<List<int>> faceList = new List<List<int>>();
            int vertexIndex = 0;

            //could use this if need be in the future for processing binary files too, but in the end you could just check for binary files and this ASCII object is owned by another thread
            //if (ASCII.IsChecked == true)
            //{
                numberCruncher.ReportProgress(0, "Counting lines in .stl file...");
                double lines = 0;
                double loops = 0;
                using (StreamReader counter = new StreamReader(models.FirstOrDefault()))
                {
                    while (counter.Peek() >= 0)
                    {
                        lines++;
                        counter.ReadLine();
                    }
                    counter.Close();
                    loops = (lines - 2) / 7; 
                }

                double currentLoop = 0;
                numberCruncher.ReportProgress(5,string.Format("Converting and sorting .stl file... current loop: {0} of {1}", currentLoop, loops));


                stlFile = new StreamReader(models.FirstOrDefault());
                stlFile.ReadLine();                                         //solid Default
                while (stlFile.ReadLine().Split(' ').First() != "endsolid")  //  facet normal
                {
                    currentLoop++;
                    stlFile.ReadLine();                                     //    outer loop
                    string[] vertex1 = stlFile.ReadLine().TrimStart(' ').Split(' '); //vertex
                    string[] vertex2 = stlFile.ReadLine().TrimStart(' ').Split(' '); //vertex
                    string[] vertex3 = stlFile.ReadLine().TrimStart(' ').Split(' '); //vertex
                    List<double> tempVertex = new List<double>();
                    List<int> tempFace = new List<int>();

                    //first vertex
                    for (int i = 1; i < 4; i++)
                        tempVertex.Add(Convert.ToDouble(vertex1[i]));
                    vertexIndex = indexFound(vertexList, tempVertex);
                    if (vertexIndex == -1)
                    {
                        vertexIndex = vertexList.Count;
                        vertexList.Add(tempVertex);
                    }
                    tempFace.Add(vertexIndex);
                    tempVertex = new List<double>();    //make a new set of doubles to record
                                                        //second vertex
                    for (int i = 1; i < 4; i++)
                        tempVertex.Add(Convert.ToDouble(vertex2[i]));
                    vertexIndex = indexFound(vertexList, tempVertex);
                    if (vertexIndex == -1)
                    {
                        vertexIndex = vertexList.Count;
                        vertexList.Add(tempVertex);
                    }
                    tempFace.Add(vertexIndex);
                    tempVertex = new List<double>();    //make a new set of doubles to record
                                                        //third vertex
                    for (int i = 1; i < 4; i++)
                        tempVertex.Add(Convert.ToDouble(vertex3[i]));
                    vertexIndex = indexFound(vertexList, tempVertex);
                    if (vertexIndex == -1)
                    {
                        vertexIndex = vertexList.Count;
                        vertexList.Add(tempVertex);
                    }
                    tempFace.Add(vertexIndex);
                    tempVertex = new List<double>();    //make a new set of doubles to record
                                                        //vertecies added to triangle, add the triangle to the triangle list
                    faceList.Add(tempFace);
                    tempFace = new List<int>(); //new triangle without clearing old one
                                                //proceed to the next set
                    stlFile.ReadLine(); //endloop
                    stlFile.ReadLine(); //endfacet
                    numberCruncher.ReportProgress((int)((currentLoop/loops)*90) + 5, string.Format("Converting and sorting .stl file... current loop: {0} of {1}", currentLoop, loops)); //these loops will acount for ~90% of the work I think
                }
                stlFile.Close(); //we're done with this file, it is now in memory
            //}

            double maxX = 0.0;
            double minX = 0.0;
            double maxY = 0.0;
            double minY = 0.0;
            double maxZ = 0.0;
            double minZ = 0.0;
            foreach (List<double> vertex in vertexList)
            {
                //translate
                vertex[0] += 12;        //x shift before flip
                vertex[2] -= 10;        //z shift before flip

                //flip
                vertex[0] = -vertex[0];   //x is flipped so that positive is forward in the plane frame
                vertex[2] = -vertex[2];   //z is flipped so that positive is down in the plane frame

                //scale
                vertex[0] /= 6.418734;
                vertex[1] /= 6.418734;
                vertex[2] /= 6.418734;
                if (vertex[0] > maxX)
                {
                    maxX = vertex[1];
                }
                if (vertex[0] < minX)
                {
                    minX = vertex[1];
                }
                if (vertex[1] > maxY)
                {
                    maxY = vertex[1];
                }
                if (vertex[1] < minY)
                {
                    minY = vertex[1];
                }
                if (vertex[2] > maxZ)
                {
                    maxZ = vertex[1];
                }
                if (vertex[2] < minZ)
                {
                    minZ = vertex[1];
                }
            }
            numberCruncher.ReportProgress(96, "Writing Verticies \"V\" Matrix...");

            //now we are ready to write the .m file!
            //write the first lines of the .m file
            mFile.WriteLine("%=======================================================================");
            mFile.WriteLine("% File:        defineVehicleBody.m");
            mFile.WriteLine("% Description: Automatically Generated from " + models.FirstOrDefault());
            mFile.WriteLine("%              \tAuto-generation program written by Michael Scott Christensen 2016");
            mFile.WriteLine("%              \tModel Contains:");
            mFile.WriteLine("%              \t\t\t-" + vertexList.Count + " unique verticies");
            mFile.WriteLine("%              \t\t\t-" + faceList.Count + " unique faces");
            mFile.WriteLine("%              \t\t\t-a whole lotta green");
            mFile.WriteLine("%              \t\t\t-minimum in x: " + minX + ", maximum in x: " + maxX);
            mFile.WriteLine("%              \t\t\t-minimum in y: " + minY + ", maximum in x: " + maxY);
            mFile.WriteLine("%              \t\t\t-minimum in z: " + minZ + ", maximum in x: " + maxZ);
            mFile.WriteLine("%=======================================================================");
            mFile.WriteLine("function [V,F,facecolors] = defineVehicleBody");
            mFile.WriteLine();

            //Define the vertices 
            mFile.WriteLine("% Define Verticies");
            mFile.WriteLine("V = [...");

            foreach (List<double> vertex in vertexList)
            {
                mFile.WriteLine("    " + vertex[0] + ", " + vertex[1] + ", " + vertex[2] + ";...");
            }
            mFile.WriteLine("    ]';");
            mFile.WriteLine();
            numberCruncher.ReportProgress(97, "Writing Faces \"F\" Matrix...");

            //define faces
            mFile.WriteLine("% Define Faces");
            mFile.WriteLine("F = [...");
            foreach (List<int> face in faceList)
            {
                mFile.WriteLine("    " + (face[0] + 1) + ", " + (face[1] + 1) + ", " + (face[2] + 1) + ";...");
            }
            mFile.WriteLine("    ];");
            mFile.WriteLine();
            numberCruncher.ReportProgress(98, "Writing Colors Matrix...");

            //define color choices
            mFile.WriteLine("myred = [1, 0, 0];");
            mFile.WriteLine("myyellow = [1, 1, 0];");
            mFile.WriteLine("mygreen = [0, 1, 0];");
            mFile.WriteLine("mycyan = [0, 1, 1];");
            mFile.WriteLine("myblue = [0, 0, 1];");
            mFile.WriteLine("mygrey = [0.5, 0.5, 0.5];");
            mFile.WriteLine();

            mFile.WriteLine("% Define Faces");
            mFile.WriteLine("facecolors = [...");
            for (int i = 0; i < faceList.Count; i++)
            {
                mFile.WriteLine("    mygrey;...");
            }
            mFile.WriteLine("    ];");
            numberCruncher.ReportProgress(99, "Closing Files...");

            //finish up and close the files
            mFile.WriteLine("end");
            mFile.Close();
            stlFile.Close();
        }

        private void numberCruncher_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("File has been generated!");
            myProgressBar.Value = 100;
            myTextBox.Text = "File Generation Complete.";
        }

        private void numberCruncher_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            myProgressBar.Value = e.ProgressPercentage;
            myTextBox.Text = (string)(e.UserState);
        }

        private int indexFound(List<List<double>> listOfVerticies, List<double> vertex)
        {
            int i = 0;
            if (!listOfVerticies.Contains(vertex))
                return -1;
            else
            {
                return listOfVerticies.IndexOf(vertex);
            }
        }
        #endregion
    }
}
