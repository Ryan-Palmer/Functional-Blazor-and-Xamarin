namespace FunctionalBlazor.Common

module Ioc =
    open System.Collections.Generic

    type IFunctionRoot = 
        abstract member Functions : IDictionary<string, obj>

