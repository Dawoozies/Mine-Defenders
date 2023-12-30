using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenu
{
    public int menuNumber { get; }
    public bool isOpen { get; }
    public void Open();
    public void Close();
}
