using PureMVC.Patterns.Command;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 启动时初始化最初必须的Model、View、Controller
/// </summary>
public class StartupInitializeCommand : MacroCommand
{
    
    protected override void InitializeMacroCommand()
    {
        AddSubCommand(() => new InitializeModelCommand());
        AddSubCommand(() => new InitializeControllerCommand());
    }

}