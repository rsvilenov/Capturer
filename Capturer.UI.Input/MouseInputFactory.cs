using System;
using System.Collections.Generic;
using System.Linq;
using Capturer.UI.Input.Enums;
using Capturer.UI.Input.Interfaces;
using Capturer.UI.Input.MouseInputs;

namespace Capturer.UI.Input
{
    public class MouseInputFactory
    {
        private static Dictionary<MouseInputType, Type> _mouseInputTypes = new Dictionary<MouseInputType, Type>
        {
            {MouseInputType.LLHookMouseInput,  typeof(LLHookMouseInput)},
            {MouseInputType.WPFBaseMouseInput,  typeof(WPFBaseMouseInput)},
            {MouseInputType.WPFCaptureMouseInput,  typeof(WPFCaptureMouseInput)}
        };

        public static IMouseInput GetMouseInput(MouseInputType mouseInputType)
        {
            var classType = _mouseInputTypes.
                SingleOrDefault(mit => mit.Key == mouseInputType).Value;

            IMouseInput instance = null;
            if (classType != null)
            {
                instance = (IMouseInput)Activator.CreateInstance(classType);
            }
            return instance;
        }
    }

}
