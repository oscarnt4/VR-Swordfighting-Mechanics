public interface IDamageable
{
    void TakeDamage(float amount);
    void Heal(float amount);
    float CurrentHealth { get; }
    float MaxHealth { get; }
}
