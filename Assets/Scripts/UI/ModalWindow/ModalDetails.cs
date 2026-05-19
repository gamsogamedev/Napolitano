using System;

public class ModalDetails
{
    public string titleText;
    public string messageText;
    
    public string confirmText = "OK";
    public Action onConfirm;
    
    public string cancelText; 
    public Action onCancel;
}
