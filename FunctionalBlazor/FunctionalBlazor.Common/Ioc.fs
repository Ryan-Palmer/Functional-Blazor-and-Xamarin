namespace FunctionalBlazor.Common

module Ioc =
    open System
    open Microsoft.Extensions.DependencyInjection
    open System.Collections.Generic

    type IFunctionRoot = 
        abstract member Functions : IDictionary<string, obj>

