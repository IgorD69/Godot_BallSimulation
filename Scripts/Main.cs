using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class Main : Node2D
{

    [Export] public float Radius { get; set; } = 30.0f;

    private readonly List<Ball> _balls = new();
    public List<Ball> ActiveBalls = new();

    public Color RandColor;

    public float Frequency = 0.01f;
    Vector2 velocityOverTime;


    public sealed class Ball
    {
        public float radius;
        public Vector2 pos;
        public Color color;

        public Vector2 velocityVector;
        public float ApplyColisionImpulse;

        public float _BallInteractRadius;


    }


    public override void _Ready()
    {
        {
            var b = InstatiateBall();
            b.pos = new Vector2(150, 150);
        }



        var r = new RandomNumberGenerator();
        var screenSize = GetViewport().GetVisibleRect().Size;

        for (int i = 0; i < 200; i++)
        {
            var b = InstatiateBall();
            var x = r.RandfRange(b.radius, screenSize.X - b.radius);
            var y = r.RandfRange(b.radius, screenSize.Y - b.radius);

            var RandomVelocity = r.RandfRange(0, 5);

            b.pos = new(x, y);
            b.velocityVector = new(0, RandomVelocity);
            b.radius = ((float)r.RandfRange(10, Radius));
        }
    }



    public Ball InstatiateBall()
    {
        Ball ball = new();
        _balls.Add(ball);

        ball.radius = Radius;
        var screenSize = GetViewport().GetVisibleRect().Size;
        var pos = screenSize / 2;
        ball.pos = pos;
        ball.velocityVector = new Vector2(0, 1);




        ball._BallInteractRadius = Radius * 2;


        return ball;
    }




    public override void _Draw()
    {
        foreach (var ball in _balls)
            DrawCircle(ball.pos, ball.radius, color: ball.color);
    }


    public override void _Process(double delta)
    {
        var screenSize = GetViewport().GetVisibleRect().Size;
        var velocityOverTime = 0.5f;
        ResolveCollisions();





        foreach (var ball in _balls)
        {
            {
                ball.color = RandColor;
                const double GForce = 9.8;
                double Vec = delta * GForce;
                ball.velocityVector.Y += (float)Vec;
                SetRandomColor(ball);

            }


            //Screen Colision If Statmant
            {

                if (ball.pos.Y + ball.radius > screenSize.Y)
                {
                    ball.pos.Y = screenSize.Y - ball.radius;

                    if (ball.velocityVector.Y > 0)
                    {
                        ball.velocityVector.Y *= -velocityOverTime;

                        if (Math.Abs(ball.velocityVector.Y) < 0.5f) ball.velocityVector.Y = 0;
                    }
                }

                if (ball.pos.Y - ball.radius < 0)
                {
                    ball.pos.Y = ball.radius;
                    if (ball.velocityVector.Y < 0)
                    {
                        ball.velocityVector.Y *= -velocityOverTime;
                    }
                }

                if (ball.pos.X + ball.radius > screenSize.X)
                {
                    ball.pos.X = screenSize.X - ball.radius;
                    if (ball.velocityVector.X > 0)
                    {
                        ball.velocityVector.X *= -velocityOverTime;
                    }
                }

                if (ball.pos.X - ball.radius < 0)
                {
                    ball.pos.X = ball.radius;
                    if (ball.velocityVector.X < 0)
                    {
                        ball.velocityVector.X *= -velocityOverTime;
                    }
                }
            }



            ball.pos += ball.velocityVector;

        }

        QueueRedraw();


    }


    public void SetRandomColor(Ball ball)
    {

        var r = new RandomNumberGenerator();
        RandColor = new(
             (float)r.RandfRange(0, 1),
             (float)r.RandfRange(0, 1),
             (float)r.RandfRange(0, 1)
         );

        ball.color = RandColor;
    }
    public void ResolveCollisions()

    {
        float friction = 0.8f;

        for (int i = 0; i < _balls.Count; i++)
        {
            Ball ballA = _balls[i];

            for (int j = i + 1; j < _balls.Count; j++)
            {
                Ball ballB = _balls[j];

                float distance = ballA.pos.DistanceTo(ballB.pos);
                float minDistance = ballA.radius + ballB.radius;

                if (distance < minDistance)
                {
                    float overlap = minDistance - distance;
                    Vector2 collisionNormal = (ballB.pos - ballA.pos).Normalized();

                    ballA.pos -= collisionNormal * (overlap / 2);
                    ballB.pos += collisionNormal * (overlap / 2);

                    ballA.velocityVector = ballA.velocityVector.Bounce(collisionNormal) * friction;
                    ballB.velocityVector = ballB.velocityVector.Bounce(-collisionNormal) * friction;
                }
            }
        }
    }


    public void ApplyImpulse(Vector2 clickPosition, Ball ball)
    {
        Vector2 direction = (ball.pos - clickPosition).Normalized();

        float force = 50.0f;

        ball.velocityVector += direction * force;
    }

    public void FluctuateSize(Ball ball)
    {

        var t = Time.GetTicksMsec();
        var sin = Math.Sin(t * Frequency);
        sin = (sin + 1) / 2;
        ball.radius = (float)(Radius * sin);

    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            Vector2 mousePos = GetLocalMousePosition();

            foreach (var ball in _balls)
            {
                if (mousePos.DistanceTo(ball.pos) < ball.radius)
                {
                    ApplyImpulse(mousePos, ball);
                    break;
                }
            }
        }

        if (@event.IsActionPressed("ui_cancel")) GetTree().Quit();
        if (@event.IsActionPressed("Reload")) GetTree().ReloadCurrentScene();
    }
}
