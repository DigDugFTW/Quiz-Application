using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp
{
    class TermSet
    {
        public string TermSetName
        {
            set;get;
        }
            
        
       
        private int timeDelay = 0;
        public int TimeDelay
        {
            set
            {
                if (value >= 0 && value <= 60)
                    timeDelay = value;
                else
                    timeDelay = 6;
            }
            get => timeDelay;
        }

        // check equality
        public override bool Equals(object obj)
        {
            TermSet term = obj as TermSet;
            term = new TermSet();
            return TermSetName == term.TermSetName && TimeDelay == term.TimeDelay;
        }
        public List<TermGroup> Terms
        {
            set;get;
        }
        
        
        // For ordering in collections
        public override int GetHashCode()
        {
            return TermSetName.GetHashCode() ^ TimeDelay.GetHashCode();
        }

        public static bool operator ==(TermSet a, TermSet b)
        {
            return a.TermSetName == b.TermSetName && a.TimeDelay == b.TimeDelay;
        }
        public static bool operator !=(TermSet a, TermSet b)
        {
            return !(a.TermSetName == b.TermSetName && a.TimeDelay == b.TimeDelay);
        }
        public override string ToString()
        {
            if (TimeDelay == 0)
                return $"Name: {TermSetName} Untimed, #Questions: {Terms.Count}";
            else
                return $"Name: {TermSetName} Delay: {TimeDelay}s  #Questions: {Terms.Count}";
        }
    }
    public class TermGroup
    {
        public string Term
        {
            set; get;
        }
        public string Definition
        {
            set; get;
        }
        public override string ToString()
        {
            return $"{Term} --> {Definition}";
        }
    }
}
