using System;
using System.Collections.Generic;
using UnityEngine;

using ConstrainedObject = FXModuleLookAtConstraint.ConstrainedObject;

namespace KatnisssPartsPack.PartModules
{
    public class FXModuleMODELLookAtConstraint : PartModule
    {
        /*[Serializable]
        public class ConstrainedObject
        {
            public List<Transform> movers; // object(s) that are moved to look at target

            public Transform target; // target

            public string targetName; // serialization stuff
            public string rotatorsName; // serialization stuff

            public Vector3 rotationAxis;

            public ConstrainedObject()
            {
                movers = new List<Transform>();
            }

            public void Load( ConfigNode node )
            {
                targetName = node.GetValue( "targetName" );
                rotatorsName = node.GetValue( "rotatorsName" );
            }

            public void Save( ConfigNode node )
            {
                node.AddValue( "targetName", targetName );
                node.AddValue( "rotatorsName", rotatorsName );
            }
        }*/

        public enum TrackMode
        {
            FixedUpdate,
            Update,
            LateUpdate
        }

        public TrackMode trackingMode;

        [KSPField]
        public string trackingModeString = "LateUpdate";

        [KSPField]
        public bool runInEditor = true;

        public List<ConstrainedObject> ObjectsList;

        private bool _initialized;

        public override void OnStart( StartState state )
        {
            if( state != StartState.Editor )
            {
                _initialized = true;
            }
            if( runInEditor )
            {
                _initialized = true;
            }
        }

        private void Update()
        {
            if( trackingMode == TrackMode.Update )
            {
                Track();
            }
        }

        private void FixedUpdate()
        {
            if( trackingMode == TrackMode.FixedUpdate )
            {
                Track();
            }
        }

        private void LateUpdate()
        {
            if( trackingMode == TrackMode.LateUpdate )
            {
                Track();
            }
        }

        public override void OnConstructionModeUpdate()
        {
            base.OnConstructionModeUpdate();
            if( trackingMode == TrackMode.Update || trackingMode == TrackMode.LateUpdate )
            {
                Track();
            }
        }

        public override void OnConstructionModeFixedUpdate()
        {
            base.OnConstructionModeFixedUpdate();
            if( trackingMode == TrackMode.FixedUpdate )
            {
                Track();
            }
        }

        public void Track()
        {
            if( !_initialized )
            {
                return;
            }

            foreach( var constrainedObject in ObjectsList )
            {
                foreach( var mover in constrainedObject.movers )
                {
                    mover.LookAt( constrainedObject.target, base.transform.up );
                }
            }
        }

        public void SetupObjects( ConstrainedObject obj )
        {
#warning TODO - find only the targets that are parented to the same MODEL as the rotators.

            // <part>
            // - model
            // - - <MODEL{},1>
            // - - <MODEL{},2>
            // - - ...

            obj.target = base.part.FindModelTransform( obj.targetName );
            obj.movers = new List<Transform>( base.part.FindModelTransforms( obj.rotatorsName ) );

            if( obj.target != null && obj.movers.Count >= 1 )
            {
                ObjectsList.Add( obj );
            }
        }

        public override void OnLoad( ConfigNode node )
        {
            if( ObjectsList == null )
            {
                ObjectsList = new List<ConstrainedObject>();
            }

            trackingMode = (TrackMode)Enum.Parse( typeof( TrackMode ), trackingModeString );

            if( !node.HasNode( "CONSTRAINLOOKFX" ) )
            {
                return;
            }

            ObjectsList.Clear();

            foreach( ConfigNode configNode in node.nodes )
            {
                if( configNode.name == "CONSTRAINLOOKFX" )
                {
                    ConstrainedObject constrainedObject = new ConstrainedObject();
                    constrainedObject.Load( configNode );
                    SetupObjects( constrainedObject );
                }
            }
            Debug.Log( $"[KPP] ObjectsList = {ObjectsList}" );
        }
    }
}