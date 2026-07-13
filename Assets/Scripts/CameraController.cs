using Player;
using UnityEngine;

/// <summary>
/// Câmera estática que replica exatamente o enquadramento da Main Camera da cena
/// e só dá zoom-out quando algum jogador sairia do quadro.
///
/// Regras:
/// - O tamanho mínimo é o tamanho ortográfico inicial da câmera (a Main Camera da
///   cena). Enquanto todos os jogadores couberem nesse quadro, nada muda.
/// - Nunca faz pan lateral: o X fica travado no valor inicial e a câmera apenas
///   alarga simetricamente ao dar zoom-out.
/// - A borda inferior fica ancorada (bottom = baseY - baseSize constante), então o
///   zoom-out cresce para cima e para os lados, nunca revelando abaixo do chão.
///
/// Assume câmera ortográfica sem tilt (setup 2D), como a Main Camera das cenas.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Zoom")]
    [Tooltip("Tamanho ortográfico máximo ao qual a câmera pode dar zoom-out. " +
             "O mínimo é sempre o tamanho inicial da câmera (a Main Camera da cena).")]
    [SerializeField] private float maxOrthographicSize = 12f;

    [Header("Margens (unidades de mundo)")]
    [Tooltip("Folga horizontal mantida entre o jogador mais distante e a borda da tela.")]
    [SerializeField] private float horizontalPadding = 1.5f;
    [Tooltip("Folga vertical mantida acima do jogador mais alto.")]
    [SerializeField] private float verticalPadding = 1.5f;

    [Header("Suavização")]
    [Tooltip("Tempo de suavização do zoom em segundos (0 = instantâneo).")]
    [SerializeField] private float zoomSmoothTime = 0.3f;

    private Camera _camera;

    // Pose base capturada no início: é exatamente a Main Camera da cena.
    private float _baseSize;      // tamanho ortográfico mínimo (== Main Camera)
    private float _baseX;         // X fixo (sem pan lateral)
    private float _baseBottom;    // borda inferior fixa (ancora no chão)
    private float _baseZ;
    private Quaternion _baseRotation;

    private float _currentSize;
    private float _zoomVelocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        var pos = transform.position;
        _baseSize = _camera.orthographicSize;
        _baseX = pos.x;
        _baseBottom = pos.y - _baseSize;
        _baseZ = pos.z;
        _baseRotation = transform.rotation;

        _currentSize = _baseSize;
        if (maxOrthographicSize < _baseSize) maxOrthographicSize = _baseSize;
    }

    private void LateUpdate()
    {
        float targetSize = ComputeRequiredSize();

        _currentSize = zoomSmoothTime > 0f
            ? Mathf.SmoothDamp(_currentSize, targetSize, ref _zoomVelocity, zoomSmoothTime)
            : targetSize;

        ApplyPose(_currentSize);
    }

    /// <summary>
    /// Menor tamanho ortográfico (>= base) que mantém todos os jogadores ativos
    /// dentro do quadro, considerando o X fixo e a borda inferior ancorada.
    /// </summary>
    private float ComputeRequiredSize()
    {
        float required = _baseSize;
        float aspect = Mathf.Max(_camera.aspect, 0.0001f); // largura / altura

        foreach (var kvp in PlayerController.AllPlayers)
        {
            var player = kvp.Value;
            if (player == null) continue;
            if (player.NetworkedStateType == PlayerController.PlayerStateType.Spoon) continue;

            Vector3 p = player.transform.position;

            // Horizontal: meiaLargura = size * aspect >= |x - baseX| + folga
            float neededForX = (Mathf.Abs(p.x - _baseX) + horizontalPadding) / aspect;

            // Vertical: bordaSuperior = baseBottom + 2*size >= y + folga
            // (a borda inferior é fixa, então só o topo cresce)
            float neededForY = (p.y + verticalPadding - _baseBottom) * 0.5f;

            required = Mathf.Max(required, Mathf.Max(neededForX, neededForY));
        }

        return Mathf.Clamp(required, _baseSize, maxOrthographicSize);
    }

    private void ApplyPose(float size)
    {
        _camera.orthographicSize = size;
        // Borda inferior ancorada: centerY = baseBottom + size. X e Z fixos.
        transform.position = new Vector3(_baseX, _baseBottom + size, _baseZ);
        transform.rotation = _baseRotation;
    }
}
