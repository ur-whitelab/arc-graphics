using UnityEngine;
using Rochester.ARTable.Particles;

namespace Rochester.ARTable.UI
{

    public class StructureAttractor : Structure
    {

        private ComputeAttractors ca;
        private int aIndex = -1;
        private bool placed = false;
        private bool autoplace = false;

        public override void CancelPlace()
        {
            Destroy(gameObject);
        }

        void Update()
        {
            if (autoplace)
            {
                Place();
                autoplace = false;
            }
            //only check for transform updates after we have placed.
            if (transform.hasChanged && aIndex > -1)
            {
                ca.UpdateAttractor(aIndex, new Vector2(transform.localPosition.x, transform.localPosition.y));
                transform.hasChanged = false;
            }
        }

        private void OnDestroy()
        {
            if (aIndex != -1)
            {
                ca.RemoveAttractor(aIndex);
            }
        }

        public override bool CanPlace()
        {

            return ca.ValidLocation(new Vector2(transform.position.x, transform.position.y));
        }

        public override void TryPreview()
        {
            Vector2 loc = new Vector2(transform.position.x, transform.position.y);
            bool valid = ca.ValidLocation(loc);
            if (valid)
            {
                //preview it
                if (aIndex < 0)
                    placed = Place();
                else
                    ca.UpdateAttractor(aIndex, loc);
            }
        }

        public override bool Place()
        {
            aIndex = ca.AddAttractor(new Vector2(this.transform.localPosition.x, this.transform.localPosition.y));
            return true;
        }

        void Awake()
        {
            ca = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeAttractors>();
            transform.hasChanged = false;
            autoplace = true;
        }


        public override void StartPlace()
        {
            autoplace = false;
            if(aIndex != -1)
            {
                ca.RemoveAttractor(aIndex);
            }
        }

    }

}