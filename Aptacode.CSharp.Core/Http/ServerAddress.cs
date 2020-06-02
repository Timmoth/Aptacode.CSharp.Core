﻿namespace Aptacode.CSharp.Core.Http
{
    public class ServerAddress
    {
        public string Protocol { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }

        public override string ToString()
        {
            return $"{Protocol}://{Address}:{Port}";
        }
    }
}