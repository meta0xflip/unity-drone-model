using Hakoniwa.PluggableAsset.Communication.Connector;
using Hakoniwa.PluggableAsset.Communication.Pdu;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hakoniwa.PluggableAsset.Assets.Robot.Parts
{
    public class Px4HobberControl : MonoBehaviour, IRobotPartsController, IRobotPartsConfig
    {
        private GameObject root;
        private string root_name;
        private PduIoConnector pdu_io;
        private IPduReader pdu_reader;

        public Rigidbody my_body;
        public GameObject motor_fl;
        public GameObject motor_fr;
        public GameObject motor_bl;
        public GameObject motor_br;

        private Px4Hobber motor_parts_fl;
        private Px4Hobber motor_parts_fr;
        private Px4Hobber motor_parts_bl;
        private Px4Hobber motor_parts_br;

        public string topic_type = "hako_mavlink_msgs/HakoHilActuatorControls";
        public string topic_name = "hobber_control";
        public int update_cycle = 1;
        public IoMethod io_method = IoMethod.SHM;
        public CommMethod comm_method = CommMethod.DIRECT;
        public RoboPartsConfigData[] GetRoboPartsConfig()
        {
            RoboPartsConfigData[] configs = new RoboPartsConfigData[1];
            configs[0] = new RoboPartsConfigData();
            configs[0].io_dir = IoDir.READ;
            configs[0].io_method = this.io_method;
            configs[0].value.org_name = this.topic_name;
            configs[0].value.type = this.topic_type;
            configs[0].value.class_name = ConstantValues.pdu_reader_class;
            configs[0].value.conv_class_name = ConstantValues.conv_pdu_reader_class;
            configs[0].value.pdu_size = 88;
            configs[0].value.write_cycle = this.update_cycle;
            configs[0].value.method_type = this.comm_method.ToString();
            return configs;
        }
        public float keisu = 2.0f;
        public float friction_k = 1.0f;
        public float rotationalFriction_k = 1.0f;
        private void DoSpeedControl()
        {
            Vector3 velocity = new Vector3(my_body.velocity.x, my_body.velocity.y, my_body.velocity.z);

            Vector3 deceleration = -velocity.normalized * friction_k;
            my_body.AddForce(deceleration, ForceMode.Acceleration);
            motor_parts_fr.DoFriction(deceleration);
            motor_parts_fl.DoFriction(deceleration);
            motor_parts_br.DoFriction(deceleration);
            motor_parts_bl.DoFriction(deceleration);


            Vector3 angularVelocity = my_body.angularVelocity;
            Vector3 angularDecelerationTorque = -angularVelocity.normalized * angularVelocity.magnitude * rotationalFriction_k;
            my_body.AddTorque(angularDecelerationTorque, ForceMode.Force);

        }

        public float yawForce = 0.1f;
        public float pitchForce = 0.1f;
        public float rollForce = 0.1f;
        public float thrust_base = 1.0f;
        public void DoControl()
        {
            float[] controls = this.pdu_reader.GetReadOps().GetDataFloat32Array("controls");

#if true
            float fl = controls[0];
            float fr = controls[1];
            float bl = controls[2];
            float br = controls[3];

            float throttle = (keisu * ((fl + fr + bl + br)/4.0f)) + thrust_base;

            motor_parts_fr.AddForce(throttle);
            motor_parts_fl.AddForce(throttle);
            motor_parts_br.AddForce(throttle);
            motor_parts_bl.AddForce(throttle);

            //unity y axis
            float yaw = (fl + br) - (fr + bl);
            my_body.AddTorque(transform.up * yaw * yawForce);

            //unity x axis
            float pitch = (fl + fr) - (bl + br);
            my_body.AddTorque(transform.forward * pitch * pitchForce);

            //z axis
            float roll = (fr + br) - (fl + bl);
            my_body.AddTorque(transform.right * roll * rollForce);
#else
            float roll = controls[0];
            float pitch =  -controls[1];
            float yaw = controls[2];
            float throttle = (keisu * controls[2]) + thrust_base;

            motor_parts_fr.AddForce(throttle);
            motor_parts_fl.AddForce(throttle);
            motor_parts_br.AddForce(throttle);
            motor_parts_bl.AddForce(throttle);

            roll = roll - 0.5f;
            pitch = pitch - 0.5f;
            yaw = yaw - 0.5f;
            //y axis
            //my_body.AddTorque(transform.up * yaw * yawForce);
            //z axis
            //my_body.AddTorque(transform.right * pitch * pitchForce);
            //x axis
            //my_body.AddTorque(transform.forward * roll * rollForce);
#endif


            DoSpeedControl();
        }

        public void Initialize(System.Object obj)
        {
            GameObject tmp = null;
            try
            {
                tmp = obj as GameObject;
            }
            catch (InvalidCastException e)
            {
                Debug.LogError("Initialize error: " + e.Message);
                return;
            }

            if (this.root == null)
            {
                this.root = tmp;
                this.root_name = string.Copy(this.root.transform.name);
                this.pdu_io = PduIoConnector.Get(root_name);
                if (this.pdu_io == null)
                {
                    throw new ArgumentException("can not found pdu_io:" + root_name);
                }
                var pdu_reader_name = root_name + "_" + this.topic_name + "Pdu";
                this.pdu_reader = this.pdu_io.GetReader(pdu_reader_name);
                if (this.pdu_reader == null)
                {
                    throw new ArgumentException("can not found pdu_reader:" + pdu_reader_name);
                }
                motor_parts_fr = motor_fr.GetComponent<Px4Hobber>();
                motor_parts_fl = motor_fl.GetComponent<Px4Hobber>();
                motor_parts_br = motor_br.GetComponent<Px4Hobber>();
                motor_parts_bl = motor_bl.GetComponent<Px4Hobber>();
            }
            motor_parts_fr.Initialize(motor_fr);
            motor_parts_fl.Initialize(motor_fl);
            motor_parts_br.Initialize(motor_br);
            motor_parts_bl.Initialize(motor_bl);
        }

        public RosTopicMessageConfig[] getRosConfig()
        {
            RosTopicMessageConfig[] cfg = new RosTopicMessageConfig[1];
            cfg[0] = new RosTopicMessageConfig();
            cfg[0].topic_message_name = this.topic_name;
            cfg[0].topic_type_name = this.topic_type;
            cfg[0].sub = true;
            return cfg;
        }

    }

}

