   M S B A M L   `   `   `  ���� J  FKinectVision360, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:KinectVision360KinectVision360       KinectVision360.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     h dMicrosoft.Kinect.Toolkit.Controls, Version=1.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35_ [Microsoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e352k(http://schemas.microsoft.com/kinect/2013   +localclr-namespace:KinectVision360   -     � Title$  KinectVision360��$	��950��$
��1300�� � WindowState$ 	Maximized=� � Icon$ Resources\kv360icon.ico�� � 	Resources    MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value��%�   &	        &	      &	 *     '�� ?     �� $ 	#ff6e6e6e���� $ 	#ff52318f���� $ 	#ff00BCF2���� " ��.���  �@J�SnapsToDevicePixels . .����  �� TabStripPlacement$	 Top=�
���   �$��0q�.@��� $��Streams��.��� -   $��mainGrid��
���   �	 � ColumnDefinitions	 �� $��1* ��� �� $��1 ��� 
 � RowDefinitions
 �� $��1* ��� �� $��1* �.+��� -   $��
scrollText��$��3��$��3��.���� -   $��textOut��$��3��$��3��$
��auto��$
��auto��
������� �� MaxLines$	 100����. $��Disabled=��� -   $��
depthimage��$��0��$
��auto��$
��auto��$��Right=�$��Center=�(� $��1��$��0�� (� Stretch$ Uniform=�$��Center=�.��� -   $��MainGrid��$	��640��$	��480��.+��� -   $��
colorimage��$
��auto��$
��auto��$��Right=�$��Center=�(   "KinectVision360.FaceTrackingViewer  -     Name$ faceTrackingViewer���� -	   $��skeletonimage��$��0��$��2��$
��auto��$
��auto��$��Right=�$��Center=�$y�Hidden=��� -
   $��depthimage2��$��1��$��0��$
��auto��$
��auto��$��Stretch=�$��Center=��� -   $��colorimage2��$��1��$��1��$
��auto��$
��auto��$��Stretch=�$��Center=��� -   $��depthimage3��$��3��$��0��$
��auto��$
��auto��$
��Left=�$��Center=��� -   $��colorimage3��$��3��$��1��$
��auto��$
��auto��$
��Left=�$��Center=��� -   $��image2��$��3��$��2��$
��auto��$
��auto��$
��Left=�$��Center=��� $��0��$��3��
�������.+��� -   $��	depthVal1���� $��2��$��VCU Capstone 2016��$��Right=�$��Bottom=�$
��Bold"�$��3���� -    ��Name$ 
KinectLogo��$��1��$��2��$��Center=�$��Center=�$)��#Resources\Kinectvision360_black.png��$y�Visible=�$
��auto��$
��auto���� $��Settings��.���� -   $��stack1��$��0��$��2�� �� Orientation$ Vertical=�
�������.+��� -    ��Name$ kinect_status��$��KINECT STATUSES�����	#FF17FF00$
��Bold"��� -   $��stack2��$ 
Horizontal=�.+��� -    ��Name$ checkIR1Mode��$��2��$��IR1 Mode��$
��Auto���� -   $ checkIR2Mode��$��2��$��	IR2 Mode ��$
��Auto���� -   $ checkIR3Mode��$��2��$��	IR3 Mode ��$
��Auto���� $&�� Adjust Tilt Angle for Sensor 1: ���� -   $��tiltSlider1��$
��Left=�$��60��$
��auto�� � Maximum$ 20Q� � Minimum$	 -20Q� �� TickPlacement$ BottomRight=� �� TickFrequency$ 5Q� �� IsSnapToTickEnabled . �� $&�� Adjust Tilt Angle for Sensor 2: ���� -   $��tiltSlider2��$
��Left=�$��60��$
��auto��$ 20Q�$	 -20Q�$ BottomRight=�$ 5Q� . �� $&�� Adjust Tilt Angle for Sensor 3: ���� -   $��tiltSlider3��$
��Left=�$��60��$
��auto��$ 20Q�$	 -20Q�$ BottomRight=�$ 5Q� . �� $(��"Adjust Sensor Range for Sensor 1: ���� -   $��depthSlider1��$
��Left=�$��60��$
��auto��$ 1Q�$ 0Q�$ BottomRight=�$ 5Q� . 