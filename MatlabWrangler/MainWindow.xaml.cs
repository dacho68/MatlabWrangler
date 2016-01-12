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
        //lists to keep track of
        private BindingList<string> models = new BindingList<string>();
        private BindingList<string> filepaths = new BindingList<string>();

        public MainWindow()
        {
            InitializeComponent();
            //link up to those bindings
            modelBox.ItemsSource = models;
            outputBox.ItemsSource = filepaths;
        }
       
        private void generate_button_Click(object sender, RoutedEventArgs e)
        {
            //open files to operate on (the .m file and the .stl)
            TextWriter mFile = new StreamWriter(filepaths.FirstOrDefault());
            StreamReader stlFile = new StreamReader(models.FirstOrDefault());
            List<List<double>> vertexList = new List<List<double>>();
            List<List<int>> faceList = new List<List<int>>();
            int vertexIndex = 0;
            
            if(ASCII.IsChecked == true)
            {
                stlFile.ReadLine();                                         //solid Default
                while(stlFile.ReadLine().Split(' ').First() != "endsolid")  //  facet normal
                {
                    stlFile.ReadLine();                                     //    outer loop
                    string[] vertex1 = stlFile.ReadLine().TrimStart(' ').Split(' '); //vertex
                    string[] vertex2 = stlFile.ReadLine().TrimStart(' ').Split(' '); //vertex
                    string[] vertex3 = stlFile.ReadLine().TrimStart(' ').Split(' '); //vertex
                    List<double> tempVertex = new List<double>();
                    List<int> tempFace = new List<int>();

                    //invert the z coordinate so that positive is down
                    vertex1[3] = (-Convert.ToDouble(vertex1[3])).ToString();
                    vertex2[3] = (-Convert.ToDouble(vertex2[3])).ToString();
                    vertex3[3] = (-Convert.ToDouble(vertex3[3])).ToString();

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
                }
                stlFile.Close(); //we're done with this file, it is now in memory
            }

            double maxX = 0.0;
            double minX = 0.0;
            double maxY = 0.0;
            double minY = 0.0;
            double maxZ = 0.0;
            double minZ = 0.0;
            foreach(List<double> vertex in vertexList)
            {
                vertex[0] /= 8;
                vertex[1] /= 8;
                vertex[2] /= 8;
                if(vertex[0] > maxX)
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

            //define faces
            mFile.WriteLine("% Define Faces");
            mFile.WriteLine("F = [...");
            foreach (List<int> face in faceList)
            {
                mFile.WriteLine("    " + (face[0] + 1) + ", " + (face[1] + 1) + ", " + (face[2] + 1) + ";...");
            }
            mFile.WriteLine("    ];");
            mFile.WriteLine();

            //define color choices
            mFile.WriteLine("myred = [1, 0, 0];");
            mFile.WriteLine("myyellow = [1, 1, 0];");
            mFile.WriteLine("mygreen = [0, 1, 0];");
            mFile.WriteLine("mycyan = [0, 1, 1];");
            mFile.WriteLine("myblue = [0, 0, 1];");
            mFile.WriteLine();

            mFile.WriteLine("% Define Faces");
            mFile.WriteLine("facecolors = [...");
            for (int i = 0; i < faceList.Count; i++)
            {
                mFile.WriteLine("    mygreen;...");
            }
            mFile.WriteLine("    ];");

            //finish up and close the files
            mFile.WriteLine("end");
            mFile.Close();
            stlFile.Close();
            MessageBox.Show("File has been generated!");
        }

        private int indexFound(List<List<double>> listOfVerticies, List<double> vertex)
        {
            int i = 0;
            foreach (List<double> listVertex in listOfVerticies)
            {
                if (vertex.SequenceEqual(listVertex))
                    return i;
                i++;
            }
            return -1;
        }

        private void choose_folder_click(object sender, RoutedEventArgs e)
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
            if(openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folder = openFileDialog.FileName;
                //refresh folder list...
                string filePath = folder + "\\defineVehicleBody.m";
                filepaths.Add(filePath);
                outputBox.ItemsSource = null;
                outputBox.ItemsSource = filepaths;
            }
        }

        private void modelBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void modelBox_Drop(object sender, DragEventArgs e)
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
    }
}
