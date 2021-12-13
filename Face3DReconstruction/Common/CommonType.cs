using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face3DReconstruction.Common
{
    public enum MESH_TYPE
    {
        ORIGINAL,
        AI
    }

    public enum POS_TYPE
    {
        FACE = 0,
        KPT_POINT = 2,
        SELECTING = 1
    }
}
