using System;
using UnityEngine;

public enum ModalLayout
{
    Vertical,
    Horizontal
}

public class ModalDetails
{
    [Header("Header & Content")]
    public string title;
    public string message;
    public Sprite contentImage;
    public ModalLayout layout = ModalLayout.Vertical;

    [Header("Botão Principal")]
    public string confirmText = "Confirmar";
    public Action onConfirm;

    [Header("Botão Secundário")]
    public string cancelText; 
    public Action onCancel;

    [Header("Botão Alternativo")]
    public string alternativeText;
    public Action onAlternative;
}