
This is a WPF-based screenshot tool I wrote for fun in 2017. I had forgotten about it, but found it on my old laptop and published it here. It automatically finds the bounds of the windows and control that are shown on the user's screen and highlights them,
so the user can easily take a screenshot of a particular window or control. It achieves that via heavy reliance on Windows API methods via P/Invoke. In terms of C# code quality it is mostly spaghetti and today I wouldn't write anything like this, but visually it looks quite nice.


https://github.com/user-attachments/assets/e79e3810-018c-405f-bc7f-817a7e32e923

