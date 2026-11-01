using System;
using System.Collections.Generic;
using System.Linq;
using Capturer.UI.Input.CursorControllers;
using Capturer.UI.Input.Enums;
using Capturer.UI.Input.Interfaces;

namespace Capturer.UI.Input
{
    public class CursorControllerFactory
    {
        private static Dictionary<CursorControlMethods, Type> _cursorControllerTypes = new Dictionary<CursorControlMethods, Type>
        {
            {CursorControlMethods.System,  typeof(SystemCursorController)},
            {CursorControlMethods.WpfWindow,  typeof(WpfWindowCursorController)}
        };

        public static ICursorController GetCursorController(CursorControlMethods cursorControllerType)
        {
            var classType = _cursorControllerTypes.
                SingleOrDefault(mit => mit.Key == cursorControllerType).Value;

            ICursorController instance = null;
            if (classType != null)
            {
                instance = (ICursorController)Activator.CreateInstance(classType);
            }
            return instance;
        }
    }
}
