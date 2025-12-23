using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WeaponLaserController : MonoBehaviour
{
    private WeaponContext weaponContext;
    private PlayerState playerState;
    private LineRenderer laserRenderer;

    [SerializeField]
    private float laserDelay = 0.5f;

    [SerializeField]
    private float laserExtendSpeed = 30f;

    [SerializeField]
    private float laserWidth = 0.05f;

    private float laserLength01;
    private float currentWidth;
    private float currentLaserDelay;
    private bool wasAiming;

    private void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
        playerState = GetComponent<PlayerState>();
        laserRenderer = GetComponent<LineRenderer>();

        laserRenderer.enabled = false;
        laserRenderer.positionCount = 2;
    }

    private void Update()
    {
        HandleDelay();
        UpdateLaser();
        wasAiming = playerState.IsAiming;
    }

    private void HandleDelay()
    {
        if (playerState.IsAiming && !wasAiming)
        {
            currentLaserDelay = laserDelay;
            laserLength01 = 0f;
        }

        if (playerState.IsAiming && currentLaserDelay > 0f)
            currentLaserDelay -= Time.deltaTime;
    }

    private void UpdateLaser()
    {
        var weapon = weaponContext.CurrentWeapon;
        var model = weaponContext.CurrentWeaponModel;

        if (weapon == null || model == null || !playerState.IsAiming || currentLaserDelay > 0f)
        {
            laserRenderer.enabled = false;
            laserLength01 = 0f;
            return;
        }

        laserRenderer.enabled = true;

        Vector3 origin = model.GetMuzzlePosition();
        Vector3 direction = model.GetFireDirection();

        Vector3 targetEnd = Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            weapon.laserDistance
        )
            ? hit.point
            : origin + direction * weapon.laserDistance;

        laserLength01 = Mathf.MoveTowards(laserLength01, 1f, Time.deltaTime * laserExtendSpeed);
        currentWidth = Mathf.MoveTowards(
            currentWidth,
            laserWidth,
            Time.deltaTime * laserExtendSpeed
        );

        laserRenderer.startWidth = currentWidth;
        laserRenderer.endWidth = currentWidth;

        laserRenderer.SetPosition(0, origin);
        laserRenderer.SetPosition(1, Vector3.Lerp(origin, targetEnd, laserLength01));
    }
}
