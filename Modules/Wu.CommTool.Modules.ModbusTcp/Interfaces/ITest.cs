using CommunityToolkit.Mvvm.Input;

namespace Wu.CommTool.Modules.ModbusTcp.Interfaces;

public interface ITest
{
    IRelayCommand TestCommand { get; }
}
