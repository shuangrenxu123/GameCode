using System;
using System.Collections.Generic;

namespace FindPath
{
    [Serializable]
    public class ActorGroup
    {
        public List<Actor> actors;
        public ActorGroup()
        {
            actors = new List<Actor>();
        }
        public void Moveactors()
        {
            foreach (var actor in actors)
            {
                actor.Move();
            }
        }
        public void AddActor(Actor actor)
        {
            actors.Add(actor);
        }
        public void Remove(Actor actor)
        {
            actors.Remove(actor);
        }
    }
}
