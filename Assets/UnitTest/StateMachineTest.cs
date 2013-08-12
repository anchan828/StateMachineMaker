using StateMachineMaker;
using NUnit.Framework;

[TestFixture]
public class StateMachineTest
{
    private StateMachine<State, Transition> stateMachine;
    [SetUp]
    public void StateMachine作成()
    {
        stateMachine = new StateMachine<State, Transition>();
    }
    [Test]
    public void StateMachineが作られること()
    {
        Assert.NotNull(stateMachine);
    }

    [Test]
    public void Stateが追加されること()
    {
        stateMachine.AddState("test");
        Assert.AreEqual(stateMachine.stateCount, 1);
    }

    [Test]
    public void Stateが追加されていることが確認できること()
    {
        Assert.False(stateMachine.HasState("test"));
        stateMachine.AddState("test");
        Assert.True(stateMachine.HasState("test"));
    }

    [Test]
    public void Stateを作成したときStateのRectが重ならないこと()
    {
        State blendShapeState1 = stateMachine.AddState("test");
        State blendShapeState2 = stateMachine.AddState("test");
        State blendShapeState3 = stateMachine.AddState("test");
        Assert.AreNotEqual(blendShapeState1.position, blendShapeState2.position);
        Assert.AreNotEqual(blendShapeState2.position, blendShapeState3.position);
        Assert.AreNotEqual(blendShapeState1.position, blendShapeState3.position);
    }

    [Test]
    public void State名が重複した場合ユニークな名前に変換されること()
    {
        stateMachine.AddState("test");
        Assert.True(stateMachine.HasState("test"));
        stateMachine.AddState("test");
        Assert.True(stateMachine.HasState("test 1"));
    }

    [Test]
    public void Stateのデフォルトが必ず1つになること()
    {
        State state = stateMachine.AddState("test");

        stateMachine.SetDefault(state);

        Assert.True(state.isDefault);

        state = stateMachine.GetState(state.stateName);

        Assert.True(state.isDefault);

        stateMachine.GetAllStates().ForEach(s =>
        {
            if (s.stateName == state.stateName)
            {
                Assert.True(s.isDefault);
            }
            else
            {
                Assert.False(s.isDefault);
            }
        });
    }

    [Test]
    public void Transactionが作成されること()
    {
        stateMachine.AddTransition("from", "to");
        Assert.AreEqual(stateMachine.stateCount, 2);
        Assert.AreEqual(stateMachine.transitionCount, 1);
    }

    [Test]
    public void Transitionが追加されていることが確認できること()
    {
        Transition addTransition = stateMachine.AddTransition("from", "to");
        Assert.True(stateMachine.HasTransitionFromUniqueID(addTransition.fromStateUniqueID, addTransition.toStateNameUniqueID));
    }

    [Test]
    public void Transition名が重複したときにユニークな名前になること()
    {
        Transition blendShapeTransition = stateMachine.AddTransition("from", "to");
        Assert.AreEqual(blendShapeTransition.name, "from-to");
        blendShapeTransition = stateMachine.AddTransition("from", "to");
        Assert.AreEqual(blendShapeTransition.name, "from-to 1");
    }
}
