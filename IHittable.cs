internal interface IHittable
{
    void Hit(bool pierce, int damage, bool crit);
}