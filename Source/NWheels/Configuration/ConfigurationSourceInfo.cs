using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Configuration
{
    public class ConfigurationSourceInfo
    {
        public ConfigurationSourceInfo(ConfigurationSourceLevel level, string name)
        {
            this.Level = level;
            this.Name = name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConfigurationSourceLevel Level { get; private set; }
        public string Name { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly ConfigurationSourceInfo _s_default = new ConfigurationSourceInfo(ConfigurationSourceLevel.Code, "Default");
        private static readonly ConfigurationSourceInfo _s_program = new ConfigurationSourceInfo(ConfigurationSourceLevel.Code, "Program");
        private static readonly ConfigurationSourceInfo _s_unknown = new ConfigurationSourceInfo(ConfigurationSourceLevel.Unknown, "???");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static ConfigurationSourceInfo _s_currentSourceInUse;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IDisposable UseSource(ConfigurationSourceLevel level, string name)
        {
            return UseSource(new ConfigurationSourceInfo(level, name));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IDisposable UseSource(ConfigurationSourceInfo source)
        {
            var scope = new UsingScope(oldSource: _s_currentSourceInUse);
            _s_currentSourceInUse = source;
            return scope;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConfigurationSourceInfo Default
        {
            get { return _s_default; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConfigurationSourceInfo Program
        {
            get { return _s_program; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static ConfigurationSourceInfo CurrentSourceInUse
        {
            get
            {
                return (_s_currentSourceInUse ?? _s_unknown);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class UsingScope : IDisposable
        {
            private readonly ConfigurationSourceInfo _oldSource;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public UsingScope(ConfigurationSourceInfo oldSource)
            {
                _oldSource = oldSource;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _s_currentSourceInUse = _oldSource;
            }
        }
    }
}
