   M S B A M L   `   `   `  ����I  EMatlabWrangler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:MatlabWranglerMatlabWrangler  5   	       MatlabWrangler.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:MatlabWrangler     5   	     � Title$  Matlab Wrangler��5   	   $��275.022��6!   $	��525��62    � 
ResizeMode$ NoResize=�6>   .��5	      � .+�5
   
   �� -   5         ��Name$ label��5
      $$��Drag a .STL file into this box��6   $
��Left=�6H   $��
215,10,0,0q�6c   $	��Top=�6w   �� 5   
   $
��Left=�6   $��32��60   $��	10,46,0,0q�6<   $	��Top=�6O   $��60��6g   .+�5      	� -   5         	�Name$ Binary��5       	� 	GroupName$ STLType��6*   $��Binary��6>   	� 5      -   5        $ ASCII��5      $ STLType��6)   $��ASCII��6=    d� 	IsChecked$
 TrueY�6M   5      �� 5   
   -   5         ��Name$ modelBox��5      $
��Left=�6$   $��45��6?   $��	75,41,0,0q�6K   $	��Top=�6^   $	��414��6v    J� 	AllowDrop . 6�   �� 5   
   -   5         ��Name$ generate_button��5      $��Generate��6*   $
��Left=�6=   $��414,205,0,0q�6X   $	��Top=�6m   $��75��6�   6�   �� 5   
   -   5        $ 	outputBox��5      $��43��6%   $��
21,120,0,0q�61   $	��Top=�6E   $
��Left=�6]   $	��468��6x   �� 5   
   -   5        $ label1��5      $��Output Path��6    $
��Left=�66   $��
224,91,0,0q�6Q   $	��Top=�6e   �� 5   
   -   5        $ choose_folder_button��5      $��Choose Folder...��6/   $
��Left=�6J   $��303,205,0,0q�6e   $	��Top=�6z   $	��106��6�   6�   �� 5   
   -	   5        	 ��Name$	 	myTextBox��5      $
��Left=�6'   $��18��6B   $��
21,182,0,0q�6N   $
��Wrap=�6b   $I��CUse the ''Choose Folder...'' button to select your matlab directory��6v   $	��Top=�6�   $	��468��6�   � 5   
   -
   5        
 �Name$
 myProgressBar��5      $
��Left=�6-   $��20��6H   $��
21,205,0,0q�6T   $	��Top=�6h   $	��277��6�   5      5      