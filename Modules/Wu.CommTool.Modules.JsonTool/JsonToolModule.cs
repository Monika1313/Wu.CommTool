﻿using Prism.Modularity;
using Wu.CommTool.Modules.JsonTool.ViewModels;

namespace Wu.CommTool.Modules.JsonTool;

public class JsonToolModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<JsonToolView, JsonToolViewModel>();                                     //Json工具界面
        containerRegistry.RegisterForNavigation<JsonDataView, JsonDataViewModel>();
    }
}