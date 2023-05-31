// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        //Private Fields
        private int dg_size;
        private HashSet<string>? set;
        private Dictionary<string, HashSet<string>> dependents;
        private Dictionary<string, HashSet<string>> dependees;


        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
            set = new HashSet<string>();
            dg_size = 0;
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            //Using a private field "size" to track each addition to the DependencyGraph
            get { return dg_size; }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get 
            {
                //If the statement within if() returns true, there is a value attached to the key, so we'll grab the array and check it's length. If there is no value for the
                //key, then we just return 0.
                if( dependees.TryGetValue(s, out set))
                {
                    return set.Count;
                }
                else
                    return 0; 
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            //The statement will return true if there is a value attached to the key
            if (dependents.TryGetValue(s, out set))
            {
                if (set.Count > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            //The statement will return true if there is a value attached to the key
            if (dependees.TryGetValue(s, out set))
            {
                if (set.Count > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            //Create a List<string> to store our values
            HashSet<string> dependlist = new();
            //Check if there is a value attached to the key. If so, return the HashSet
            if( dependents.TryGetValue(s, out set))
                return set;
            //If we get here, we'll return a list, but it'll just be empty, which is fine
            return dependlist;
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            //Create a List<string> to store our values
            HashSet<string> dependlist = new();
            //Check if there is a value attached to the key. If so, return the HashSet
            if (dependees.TryGetValue(s, out set))
                return set;
            //If we get here, we'll return a list, but it'll just be empty, which is fine
            return dependlist;
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            //First, see if there is a key value pair in dependents for the key "s"
            if(dependents.TryGetValue(s, out set))
            {
                //There IS a key with values, so see if "t" is already a value
                //If we didn't find a value "t", add it
                if (!set.Contains(t))
                {
                    //Increment size of DependencyGraph
                    dg_size++;
                    //Add "t" to the values ArrayList
                    set.Add(t);
                    //Add to dependees -- we know that if the key-value pair didn't exist for dependents, then it won't for dependees. 
                    //First we have to check if the dependees list has a key-value pair for "t" as the key
                    if (dependees.TryGetValue(t, out set))
                        //Add "s" to the values ArrayList, we know it doesn't exist
                        set.Add(s);
                    else
                    {
                        //There were no values for the Key "t", so make a new ArrayList and add it
                        HashSet<string> newdependees = new() { s };
                        dependees.Add(t, newdependees);
                    }
                }
            }
            //This means that there was no existing key-value pair for Dependents with key "s"
            else
            {
                //Create a new ArrayList, add it to the key
                HashSet<string> newdependents = new(){t};
                dependents.Add(s, newdependents);
                //Add to Dependees
                if (dependees.TryGetValue(t, out set))
                    set.Add(s);
                else
                {
                    HashSet<string> newdependees = new() { s };
                    dependees.Add(t, newdependees);
                }
                //Increment size of DependencyGraph
                dg_size++;
            }
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            bool wasRemoved = false;
            //Removing from Dependents
            if (dependents.TryGetValue(s, out set))
            {
                set.Remove(t);
                wasRemoved = true;
            }

            //Removing from Dependees
            if(dependees.TryGetValue(t, out set))
            {
                set.Remove(s);
                wasRemoved = true;
            }

            //If something was removed, account for the new size
            if(wasRemoved)
                dg_size--;
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            //Check if a key-value pair exists, if it does...
            if(dependents.TryGetValue(s, out set))
            {
                //Grab a list of the values
                HashSet<string> values = (HashSet<string>)GetDependents(s);

                //Remove values one at a time (we do this because it will remove from both Dependents and Dependees, as well as account for DependencyGraph size
                foreach (string a in values)
                    RemoveDependency(s, a);

                //Add the new values
                foreach (string t in newDependents)
                    AddDependency(s, t);
            }
            //We don't have to remove anything, so just start adding
            else
            {
                foreach (string t in newDependents)
                    AddDependency(s, t);
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            //Check if a key-value pair exists, if it does...
            if (dependees.TryGetValue(s, out set))
            {
                //Grab a list of the values
                HashSet<string> values = (HashSet<string>)GetDependees(s);

                //Remove values one at a time (we do this because it will remove from both Dependents and Dependees, as well as account for DependencyGraph size
                //Note that each "Remove" or "Add" instance has reversed values -- this is because we are working on Dependees instead of dependents.
                foreach (string a in values)
                    RemoveDependency(a, s);

                //Add the new values
                foreach (string t in newDependees)
                    AddDependency(t, s);
            }
            //We don't have to remove anything, so just start adding
            else
            {
                foreach (string t in newDependees)
                    AddDependency(t, s);
            }
        }

    }

}