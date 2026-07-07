using System;

namespace VXMonster.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UnlockConditionAttribute : Attribute
    {
        protected string menuName;
        protected int order;

        public string MenuName => menuName;
        public int Order => order;

        public UnlockConditionAttribute(string menuName, int order = 0)
        {
            this.menuName = menuName;
            this.order = order;
        }
    }
}