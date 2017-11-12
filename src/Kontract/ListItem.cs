﻿using System;

namespace Kontract
{
    public class ListItem : IComparable<ListItem>
    {
        public string Text { get; }
        public object Value { get; }

        public ListItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }

        public int CompareTo(ListItem rhs)
        {
            return Text.CompareTo(rhs.Text);
        }
    }
}