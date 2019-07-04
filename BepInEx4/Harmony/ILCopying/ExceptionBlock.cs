﻿using System;

namespace Harmony.ILCopying
{
    public class ExceptionBlock
    {
        public ExceptionBlockType blockType;

        public Type catchType;

        public ExceptionBlock(ExceptionBlockType blockType, Type catchType)
        {
            this.blockType = blockType;
            this.catchType = catchType;
        }
    }
}