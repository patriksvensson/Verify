﻿using System;

namespace Verify
{
    public static partial class Global
    {
        internal static SerializationSettings serialization = new SerializationSettings();

        public static void ModifySerialization(Action<SerializationSettings> action)
        {
            action(serialization);
            serialization.RegenSettings();
        }
    }
}