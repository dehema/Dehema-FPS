using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ConfigBase
{
    public bool isLoaded = false;

    public virtual void Init()
    {
        isLoaded = true;
    }
}
