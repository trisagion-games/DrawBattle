using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"byte[]\", \"int\"][\"int\", \"byte[]\"][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"textureData\", \"player\"][\"playerNum\", \"textureData\"][]]")]
	public abstract partial class PlayerDrawBehavior : NetworkBehavior
	{
		public const byte RPC_SEND_FULL_TEXTURE = 0 + 5;
		public const byte RPC_SEND_DRAWING_COMPLETE = 1 + 5;
		public const byte RPC_SEND_SWITCH_TO_NEXT_DRAWING = 2 + 5;
		
		public PlayerDrawNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (PlayerDrawNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("SendFullTexture", SendFullTexture, typeof(byte[]), typeof(int));
			networkObject.RegisterRpc("SendDrawingComplete", SendDrawingComplete, typeof(int), typeof(byte[]));
			networkObject.RegisterRpc("SendSwitchToNextDrawing", SendSwitchToNextDrawing);

			networkObject.onDestroy += DestroyGameObject;

			if (!obj.IsOwner)
			{
				if (!skipAttachIds.ContainsKey(obj.NetworkId)){
					uint newId = obj.NetworkId + 1;
					ProcessOthers(gameObject.transform, ref newId);
				}
				else
					skipAttachIds.Remove(obj.NetworkId);
			}

			if (obj.Metadata != null)
			{
				byte transformFlags = obj.Metadata[0];

				if (transformFlags != 0)
				{
					BMSByte metadataTransform = new BMSByte();
					metadataTransform.Clone(obj.Metadata);
					metadataTransform.MoveStartIndex(1);

					if ((transformFlags & 0x01) != 0 && (transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() =>
						{
							transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform);
							transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform);
						});
					}
					else if ((transformFlags & 0x01) != 0)
					{
						MainThreadManager.Run(() => { transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform); });
					}
					else if ((transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() => { transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform); });
					}
				}
			}

			MainThreadManager.Run(() =>
			{
				NetworkStart();
				networkObject.Networker.FlushCreateActions(networkObject);
			});
		}

		protected override void CompleteRegistration()
		{
			base.CompleteRegistration();
			networkObject.ReleaseCreateBuffer();
		}

		public override void Initialize(NetWorker networker, byte[] metadata = null)
		{
			Initialize(new PlayerDrawNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new PlayerDrawNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// byte[] textureData
		/// int player
		/// </summary>
		public abstract void SendFullTexture(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int playerNum
		/// byte[] textureData
		/// </summary>
		public abstract void SendDrawingComplete(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void SendSwitchToNextDrawing(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}