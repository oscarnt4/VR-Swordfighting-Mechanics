public interface IDamaging
{
    float DamageAmount { get; }
    void InflictDamage(IDamageable target);
}