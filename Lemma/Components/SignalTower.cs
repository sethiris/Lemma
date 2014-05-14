﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Lemma.Util;
using System.Xml.Serialization;
using Lemma.Factories;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using ComponentBind;

namespace Lemma.Components
{
	public class SignalTower : Component<Main>
	{
		public Property<string> Initial = new Property<string> { Editable = true };
		public Property<Entity.Handle> Player = new Property<Entity.Handle> { Editable = false };

		private const float messageDelay = 2.0f;

		[XmlIgnore]
		public Command<Entity> PlayerEnteredRange = new Command<Entity>();
		[XmlIgnore]
		public Command<Entity> PlayerExitedRange = new Command<Entity>();

		public override void InitializeProperties()
		{
			base.InitializeProperties();
			this.PlayerEnteredRange.Action = delegate(Entity p)
			{
				Phone phone = Factory<Main>.Get<PlayerDataFactory>().Instance(this.main).Get<Phone>();

				if (!string.IsNullOrEmpty(this.Initial))
				{
					IEnumerable<DialogueForest> forests = WorldFactory.Get().GetListProperty<DialogueForest>();
					foreach (DialogueForest forest in forests)
					{
						DialogueForest.Node n = forest.GetByName(this.Initial);
						if (n != null)
						{
							if (n.type == DialogueForest.Node.Type.Choice)
								throw new Exception("Cannot start dialogue tree with a choice");
							phone.Execute(forest, n);
							if (phone.Schedules.Count == 0)
							{
								// If there are choices available, they will initiate a conversation.
								// The player should be able to pull up the phone, see the choices, and walk away without picking any of them.
								// Normally, you can't put the phone down until you've picked an answer.
								phone.WaitForAnswer.Value = false; 
							}
							break;
						}
					}
					this.Initial.Value = null;
				}

				p.GetOrMakeProperty<Entity.Handle>("SignalTower").Value = this.Entity;
			};

			this.PlayerExitedRange.Action = delegate(Entity p)
			{
				Phone phone = Factory<Main>.Get<PlayerDataFactory>().Instance(this.main).Get<Phone>();

				p.GetOrMakeProperty<Entity.Handle>("SignalTower").Value = null;
			};
		}

		public override void delete()
		{
			Entity player = this.Player.Value.Target;
			if (player != null && player.Active)
			{
				Property<Entity.Handle> signalTowerHandle = player.GetOrMakeProperty<Entity.Handle>("SignalTower");
				if (signalTowerHandle.Value.Target == this.Entity)
					signalTowerHandle.Value = null;
			}
			base.delete();
		}
	}
}
