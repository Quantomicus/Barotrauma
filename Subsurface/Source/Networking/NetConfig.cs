﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Subsurface.Networking
{
    static class NetConfig
    {
        public const int DefaultPort = 14242;

        //UpdateEntity networkevents aren't sent to clients if they're further than this from the entity
        public const float UpdateEntityDistance = 2500.0f;

        public static string MasterServerUrl = GameMain.Config.MasterServerUrl;

        //if a ragdoll is further than this from the correct position, teleport it there
        //(in sim units)
        public const float ResetRagdollDistance = 2.0f;

        //if the ragdoll is closer than this, don't try to correct its position
        public const float AllowedRagdollDistance = 0.1f;

        public const float DeleteDisconnectedTime = 10.0f;
    }
}