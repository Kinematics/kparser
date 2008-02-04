using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Simple revision of a generic list to hold TargetDetails and allow lookup by target name.
    /// </summary>
    internal class TargetDetailCollection : List<TargetDetails>
    {
        internal TargetDetails this[string targetName]
        {
            get
            {
                foreach (TargetDetails targ in this)
                {
                    if (targ.Name == targetName)
                        return targ;
                }

                TargetDetails newTarg = new TargetDetails(targetName);
                this.Add(newTarg);
                return newTarg;
            }
        }

        internal TargetDetails Add(string newTargetName)
        {
            //if (Exists(newTargetName) == true)
            //    return;

            TargetDetails newTarg = new TargetDetails(newTargetName);
            this.Add(newTarg);
            return newTarg;
        }

        internal void Add(TargetDetailCollection targetCollection)
        {
            foreach (TargetDetails target in targetCollection)
            {
                if (Exists(target.Name) == false)
                    Add(target);
            }
        }

        private bool Exists(string findTarget)
        {
            foreach (TargetDetails target in this)
            {
                if (target.Name == findTarget)
                    return true;
            }

            return false;
        }
    }
}
