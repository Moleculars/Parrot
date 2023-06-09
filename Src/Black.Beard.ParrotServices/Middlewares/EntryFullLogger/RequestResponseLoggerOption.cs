﻿using Bb.ComponentModel.Attributes;

namespace Bb.Middlewares.EntryFullLogger
{
    [ExposeClass(Context = Constants.Models.Configuration, ExposedType = typeof(RequestResponseLoggerOption), LifeCycle = IocScopeEnum.Singleton)]
    public class RequestResponseLoggerOption
    {

        public RequestResponseLoggerOption()
        {
            
        }

        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public string DateTimeFormat { get; set; }
    }


}
