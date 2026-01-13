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
    private GameObject hitPointPrefab;

    private GameObject hitPointInstance;

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

        hitPointInstance = Instantiate(hitPointPrefab);
        hitPointInstance.SetActive(false);
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
        // Condiciones para desactivar el láser
        if (!weaponContext.HasWeapon || !playerState.IsAiming || currentLaserDelay > 0f)
        {
            laserRenderer.enabled = false;
            laserLength01 = 0f;

            if (hitPointInstance != null)
                hitPointInstance.SetActive(false);

            return;
        }

        laserRenderer.enabled = true;

        Vector3 origin = weaponContext.MuzzlePosition;
        Vector3 direction = weaponContext.FireDirection;

        // Raycast para detectar impacto
        RaycastHit hit;
        bool hasHit = Physics.Raycast(
            origin,
            direction,
            out hit,
            weaponContext.LaserDistance
        );

        Vector3 targetEnd = hasHit
            ? hit.point
            : origin + direction * weaponContext.LaserDistance;

        // Animación de extensión del láser
        laserLength01 = Mathf.MoveTowards(
            laserLength01,
            1f,
            Time.deltaTime * laserExtendSpeed
        );

        // Animación del grosor
        currentWidth = Mathf.MoveTowards(
            currentWidth,
            laserWidth,
            Time.deltaTime * laserExtendSpeed
        );

        laserRenderer.startWidth = currentWidth;
        laserRenderer.endWidth = currentWidth;

        // Posiciones del LineRenderer
        laserRenderer.SetPosition(0, origin);
        laserRenderer.SetPosition(1, Vector3.Lerp(origin, targetEnd, laserLength01));

        // Punto de impacto (billboard)
        if (hitPointInstance != null)
        {
            if (hasHit)
            {
                hitPointInstance.SetActive(true);
                hitPointInstance.transform.position = hit.point;
            }
            else
            {
                hitPointInstance.SetActive(false);
            }
        }
    }
}

