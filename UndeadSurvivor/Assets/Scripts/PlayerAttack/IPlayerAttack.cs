using System.Threading;

public interface IPlayerAttack
{
    void Attack(CancellationTokenSource cts);
    void Flip();
}
