﻿using System.Collections.Generic;

namespace E.Data
{
    public class DirectoryAsyncOperation : ConnectionAsyncOperation
    {
        protected DirectoryAsyncOperation() { }

        protected double progress;
        
        public override double Progress => progress;

        public SortedList<string, DataStream.ResourceInfo> Resources { get; protected set; }

    }
}