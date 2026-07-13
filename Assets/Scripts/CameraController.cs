using Player;
using UnityEngine;

/// <summary>
/// Câmera de co-op para fases largas/longas.
///
/// - Segue o ponto médio dos jogadores (pan horizontal e vertical).
/// - Dá zoom out só quando eles se afastam demais para caber na tela. O mínimo é
///   o tamanho inicial da câmera, então uma fase pequena continua idêntica.
/// - Fica confinada aos limites da fase (leftX/rightX/floorY/ceilingY): nunca
///   mostra abaixo do chão nem além das bordas do nível.
///
/// Os limites são definidos no Inspector, por fase. Com o objeto selecionado, o
/// retângulo dos limites aparece em ciano na Scene view para facilitar o ajuste.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Limites da fase (em mundo)")]
    [Tooltip("Borda esquerda jogável (X). A câmera não passa disso.")]
    [SerializeField] private float leftX = -8f;
    [Tooltip("Borda direita jogável (X). A câmera não passa disso.")]
    [SerializeField] private float rightX = 8f;
    [Tooltip("Linha do chão (Y). A câmera nunca mostra abaixo disso.")]
    [SerializeField] private float floorY = -5f;
    [Tooltip("Teto da fase (Y). A câmera nunca mostra acima disso.")]
    [SerializeField] private float ceilingY = 15f;

    [Header("Zoom")]
    [Tooltip("Tamanho ortográfico máximo (zoom out). O mínimo é o tamanho inicial da câmera.")]
    [SerializeField] private float maxOrthographicSize = 12f;

    [Header("Margens (em mundo)")]
    [Tooltip("Folga horizontal entre o jogador mais externo e a borda da tela.")]
    [SerializeField] private float horizontalPadding = 2f;
    [Tooltip("Folga vertical entre o jogador mais externo e a borda da tela.")]
    [SerializeField] private float verticalPadding = 2f;

    [Header("Suavização (segundos)")]
    [Tooltip("Suavização do movimento de seguir os jogadores (0 = instantâneo).")]
    [SerializeField] private float followSmoothTime = 0.25f;
    [Tooltip("Suavização do zoom (0 = instantâneo).")]
    [SerializeField] private float zoomSmoothTime = 0.3f;

    private Camera _camera;
    private float _baseSize;   // tamanho ortográfico inicial (= mínimo)
    private float _baseZ;
    private Quaternion _baseRotation;

    private float _currentSize;
    private float _zoomVel;
    private Vector2 _currentCenter;
    private Vector2 _centerVel;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _baseSize = _camera.orthographicSize;
        _baseZ = transform.position.z;
        _baseRotation = transform.rotation;

        if (maxOrthographicSize < _baseSize) maxOrthographicSize = _baseSize;

        _currentSize = _baseSize;
        _currentCenter = transform.position;
    }

    private void LateUpdate()
    {
        float aspect = Mathf.Max(_camera.aspect, 0.0001f); // largura / altura

        if (TryGetPlayersBounds(out Vector2 min, out Vector2 max))
        {
            // Menor tamanho que cabe a caixa dos jogadores (+ margem), limitado ao teto.
            float halfW = (max.x - min.x) * 0.5f + horizontalPadding;
            float halfH = (max.y - min.y) * 0.5f + verticalPadding;
            float targetSize = Mathf.Max(_baseSize, halfH, halfW / aspect);
            targetSize = Mathf.Clamp(targetSize, _baseSize, maxOrthographicSize);

            Vector2 targetCenter = (min + max) * 0.5f;

            _currentSize = zoomSmoothTime > 0f
                ? Mathf.SmoothDamp(_currentSize, targetSize, ref _zoomVel, zoomSmoothTime)
                : targetSize;

            _currentCenter = followSmoothTime > 0f
                ? Vector2.SmoothDamp(_currentCenter, targetCenter, ref _centerVel, followSmoothTime)
                : targetCenter;
        }

        ApplyPose(aspect);
    }

    private bool TryGetPlayersBounds(out Vector2 min, out Vector2 max)
    {
        min = Vector2.zero;
        max = Vector2.zero;
        bool any = false;

        foreach (var kvp in PlayerController.AllPlayers)
        {
            var player = kvp.Value;
            if (player == null) continue;
            if (player.NetworkedStateType == PlayerController.PlayerStateType.Spoon) continue;

            Vector2 p = player.transform.position;
            if (!any)
            {
                min = max = p;
                any = true;
            }
            else
            {
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
        }

        return any;
    }

    private void ApplyPose(float aspect)
    {
        _camera.orthographicSize = _currentSize;

        Vector2 c = ClampToBounds(_currentCenter, _currentSize, aspect);
        transform.position = new Vector3(c.x, c.y, _baseZ);
        transform.rotation = _baseRotation;
    }

    /// <summary>
    /// Prende o centro da câmera dentro dos limites da fase. Se a fase for menor
    /// que a tela em algum eixo, centraliza nesse eixo (na vertical, prende no chão
    /// para nunca revelar abaixo dele).
    /// </summary>
    private Vector2 ClampToBounds(Vector2 center, float size, float aspect)
    {
        float halfW = size * aspect;
        float halfH = size;

        float x = (rightX - leftX <= 2f * halfW)
            ? (leftX + rightX) * 0.5f
            : Mathf.Clamp(center.x, leftX + halfW, rightX - halfW);

        float y = (ceilingY - floorY <= 2f * halfH)
            ? floorY + halfH
            : Mathf.Clamp(center.y, floorY + halfH, ceilingY - halfH);

        return new Vector2(x, y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var bl = new Vector3(leftX, floorY, 0f);
        var br = new Vector3(rightX, floorY, 0f);
        var tr = new Vector3(rightX, ceilingY, 0f);
        var tl = new Vector3(leftX, ceilingY, 0f);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
}
