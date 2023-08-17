// Generated by sprotodump. DO NOT EDIT!
// source: D:\LUWENHAI\UProject\PanelDemo\Project\Sproto\proto\/proto.s2c.sproto

using System;
using Sproto;
using System.Collections.Generic;

namespace S2C_SprotoType { 
	public class block_info : SprotoTypeBase {
		private static int max_field_count = 5;
		
		
		private Int64 _row; // tag 0
		public Int64 row {
			get { return _row; }
			set { base.has_field.set_field (0, true); _row = value; }
		}
		public bool HasRow {
			get { return base.has_field.has_field (0); }
		}

		private Int64 _col; // tag 1
		public Int64 col {
			get { return _col; }
			set { base.has_field.set_field (1, true); _col = value; }
		}
		public bool HasCol {
			get { return base.has_field.has_field (1); }
		}

		private Int64 _shape; // tag 2
		public Int64 shape {
			get { return _shape; }
			set { base.has_field.set_field (2, true); _shape = value; }
		}
		public bool HasShape {
			get { return base.has_field.has_field (2); }
		}

		private Int64 _state; // tag 3
		public Int64 state {
			get { return _state; }
			set { base.has_field.set_field (3, true); _state = value; }
		}
		public bool HasState {
			get { return base.has_field.has_field (3); }
		}

		private Int64 _frame; // tag 4
		public Int64 frame {
			get { return _frame; }
			set { base.has_field.set_field (4, true); _frame = value; }
		}
		public bool HasFrame {
			get { return base.has_field.has_field (4); }
		}

		public block_info () : base(max_field_count) {}

		public block_info (byte[] buffer) : base(max_field_count, buffer) {
			this.decode ();
		}

		protected override void decode () {
			int tag = -1;
			while (-1 != (tag = base.deserialize.read_tag ())) {
				switch (tag) {
				case 0:
					this.row = base.deserialize.read_integer ();
					break;
				case 1:
					this.col = base.deserialize.read_integer ();
					break;
				case 2:
					this.shape = base.deserialize.read_integer ();
					break;
				case 3:
					this.state = base.deserialize.read_integer ();
					break;
				case 4:
					this.frame = base.deserialize.read_integer ();
					break;
				default:
					base.deserialize.read_unknow_data ();
					break;
				}
			}
		}

		public override int encode (SprotoStream stream) {
			base.serialize.open (stream);

			if (base.has_field.has_field (0)) {
				base.serialize.write_integer (this.row, 0);
			}

			if (base.has_field.has_field (1)) {
				base.serialize.write_integer (this.col, 1);
			}

			if (base.has_field.has_field (2)) {
				base.serialize.write_integer (this.shape, 2);
			}

			if (base.has_field.has_field (3)) {
				base.serialize.write_integer (this.state, 3);
			}

			if (base.has_field.has_field (4)) {
				base.serialize.write_integer (this.frame, 4);
			}

			return base.serialize.close ();
		}
	}


	public class error {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 2;
			
			
			private Int64 _id; // tag 0
			public Int64 id {
				get { return _id; }
				set { base.has_field.set_field (0, true); _id = value; }
			}
			public bool HasId {
				get { return base.has_field.has_field (0); }
			}

			private string _txt; // tag 1
			public string txt {
				get { return _txt; }
				set { base.has_field.set_field (1, true); _txt = value; }
			}
			public bool HasTxt {
				get { return base.has_field.has_field (1); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.id = base.deserialize.read_integer ();
						break;
					case 1:
						this.txt = base.deserialize.read_string ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.id, 0);
				}

				if (base.has_field.has_field (1)) {
					base.serialize.write_string (this.txt, 1);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_block_buffer {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private string _buffer; // tag 0
			public string buffer {
				get { return _buffer; }
				set { base.has_field.set_field (0, true); _buffer = value; }
			}
			public bool HasBuffer {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.buffer = base.deserialize.read_string ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_string (this.buffer, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_garbage_buffer {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private string _buffer; // tag 0
			public string buffer {
				get { return _buffer; }
				set { base.has_field.set_field (0, true); _buffer = value; }
			}
			public bool HasBuffer {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.buffer = base.deserialize.read_string ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_string (this.buffer, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_info {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 3;
			
			
			private Int64 _e; // tag 0
			public Int64 e {
				get { return _e; }
				set { base.has_field.set_field (0, true); _e = value; }
			}
			public bool HasE {
				get { return base.has_field.has_field (0); }
			}

			private Int64 _frame; // tag 1
			public Int64 frame {
				get { return _frame; }
				set { base.has_field.set_field (1, true); _frame = value; }
			}
			public bool HasFrame {
				get { return base.has_field.has_field (1); }
			}

			private List<block_info> _blocks; // tag 2
			public List<block_info> blocks {
				get { return _blocks; }
				set { base.has_field.set_field (2, true); _blocks = value; }
			}
			public bool HasBlocks {
				get { return base.has_field.has_field (2); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.e = base.deserialize.read_integer ();
						break;
					case 1:
						this.frame = base.deserialize.read_integer ();
						break;
					case 2:
						this.blocks = base.deserialize.read_obj_list<block_info> ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.e, 0);
				}

				if (base.has_field.has_field (1)) {
					base.serialize.write_integer (this.frame, 1);
				}

				if (base.has_field.has_field (2)) {
					base.serialize.write_obj (this.blocks, 2);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_matched {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 2;
			
			
			private Int64 _frame; // tag 0
			public Int64 frame {
				get { return _frame; }
				set { base.has_field.set_field (0, true); _frame = value; }
			}
			public bool HasFrame {
				get { return base.has_field.has_field (0); }
			}

			private List<block_info> _matched_blocks; // tag 1
			public List<block_info> matched_blocks {
				get { return _matched_blocks; }
				set { base.has_field.set_field (1, true); _matched_blocks = value; }
			}
			public bool HasMatched_blocks {
				get { return base.has_field.has_field (1); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.frame = base.deserialize.read_integer ();
						break;
					case 1:
						this.matched_blocks = base.deserialize.read_obj_list<block_info> ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.frame, 0);
				}

				if (base.has_field.has_field (1)) {
					base.serialize.write_obj (this.matched_blocks, 1);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_new_row {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 2;
			
			
			private Int64 _frame; // tag 0
			public Int64 frame {
				get { return _frame; }
				set { base.has_field.set_field (0, true); _frame = value; }
			}
			public bool HasFrame {
				get { return base.has_field.has_field (0); }
			}

			private List<block_info> _row_blocks; // tag 1
			public List<block_info> row_blocks {
				get { return _row_blocks; }
				set { base.has_field.set_field (1, true); _row_blocks = value; }
			}
			public bool HasRow_blocks {
				get { return base.has_field.has_field (1); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.frame = base.deserialize.read_integer ();
						break;
					case 1:
						this.row_blocks = base.deserialize.read_obj_list<block_info> ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.frame, 0);
				}

				if (base.has_field.has_field (1)) {
					base.serialize.write_obj (this.row_blocks, 1);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_over {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 2;
			
			
			private Int64 _frame; // tag 0
			public Int64 frame {
				get { return _frame; }
				set { base.has_field.set_field (0, true); _frame = value; }
			}
			public bool HasFrame {
				get { return base.has_field.has_field (0); }
			}

			private string _winner; // tag 1
			public string winner {
				get { return _winner; }
				set { base.has_field.set_field (1, true); _winner = value; }
			}
			public bool HasWinner {
				get { return base.has_field.has_field (1); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.frame = base.deserialize.read_integer ();
						break;
					case 1:
						this.winner = base.deserialize.read_string ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.frame, 0);
				}

				if (base.has_field.has_field (1)) {
					base.serialize.write_string (this.winner, 1);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_raise {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private Int64 _frame; // tag 0
			public Int64 frame {
				get { return _frame; }
				set { base.has_field.set_field (0, true); _frame = value; }
			}
			public bool HasFrame {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.frame = base.deserialize.read_integer ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.frame, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_ready {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private List<block_info> _blocks; // tag 0
			public List<block_info> blocks {
				get { return _blocks; }
				set { base.has_field.set_field (0, true); _blocks = value; }
			}
			public bool HasBlocks {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.blocks = base.deserialize.read_obj_list<block_info> ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_obj (this.blocks, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_rollback {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private Int64 _frame; // tag 0
			public Int64 frame {
				get { return _frame; }
				set { base.has_field.set_field (0, true); _frame = value; }
			}
			public bool HasFrame {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.frame = base.deserialize.read_integer ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.frame, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_start {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private Int64 _start_time; // tag 0
			public Int64 start_time {
				get { return _start_time; }
				set { base.has_field.set_field (0, true); _start_time = value; }
			}
			public bool HasStart_time {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.start_time = base.deserialize.read_integer ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.start_time, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class game_swap {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 3;
			
			
			private Int64 _frame; // tag 0
			public Int64 frame {
				get { return _frame; }
				set { base.has_field.set_field (0, true); _frame = value; }
			}
			public bool HasFrame {
				get { return base.has_field.has_field (0); }
			}

			private block_info _block1; // tag 1
			public block_info block1 {
				get { return _block1; }
				set { base.has_field.set_field (1, true); _block1 = value; }
			}
			public bool HasBlock1 {
				get { return base.has_field.has_field (1); }
			}

			private block_info _block2; // tag 2
			public block_info block2 {
				get { return _block2; }
				set { base.has_field.set_field (2, true); _block2 = value; }
			}
			public bool HasBlock2 {
				get { return base.has_field.has_field (2); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.frame = base.deserialize.read_integer ();
						break;
					case 1:
						this.block1 = base.deserialize.read_obj<block_info> ();
						break;
					case 2:
						this.block2 = base.deserialize.read_obj<block_info> ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.frame, 0);
				}

				if (base.has_field.has_field (1)) {
					base.serialize.write_obj (this.block1, 1);
				}

				if (base.has_field.has_field (2)) {
					base.serialize.write_obj (this.block2, 2);
				}

				return base.serialize.close ();
			}
		}


	}


	public class key_value : SprotoTypeBase {
		private static int max_field_count = 2;
		
		
		private Int64 _id; // tag 0
		public Int64 id {
			get { return _id; }
			set { base.has_field.set_field (0, true); _id = value; }
		}
		public bool HasId {
			get { return base.has_field.has_field (0); }
		}

		private Int64 _value; // tag 1
		public Int64 value {
			get { return _value; }
			set { base.has_field.set_field (1, true); _value = value; }
		}
		public bool HasValue {
			get { return base.has_field.has_field (1); }
		}

		public key_value () : base(max_field_count) {}

		public key_value (byte[] buffer) : base(max_field_count, buffer) {
			this.decode ();
		}

		protected override void decode () {
			int tag = -1;
			while (-1 != (tag = base.deserialize.read_tag ())) {
				switch (tag) {
				case 0:
					this.id = base.deserialize.read_integer ();
					break;
				case 1:
					this.value = base.deserialize.read_integer ();
					break;
				default:
					base.deserialize.read_unknow_data ();
					break;
				}
			}
		}

		public override int encode (SprotoStream stream) {
			base.serialize.open (stream);

			if (base.has_field.has_field (0)) {
				base.serialize.write_integer (this.id, 0);
			}

			if (base.has_field.has_field (1)) {
				base.serialize.write_integer (this.value, 1);
			}

			return base.serialize.close ();
		}
	}


	public class matching_error {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private Int64 _type; // tag 0
			public Int64 type {
				get { return _type; }
				set { base.has_field.set_field (0, true); _type = value; }
			}
			public bool HasType {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.type = base.deserialize.read_integer ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_integer (this.type, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class matching_success {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 1;
			
			
			private List<player_info> _players; // tag 0
			public List<player_info> players {
				get { return _players; }
				set { base.has_field.set_field (0, true); _players = value; }
			}
			public bool HasPlayers {
				get { return base.has_field.has_field (0); }
			}

			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					case 0:
						this.players = base.deserialize.read_obj_list<player_info> ();
						break;
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				if (base.has_field.has_field (0)) {
					base.serialize.write_obj (this.players, 0);
				}

				return base.serialize.close ();
			}
		}


	}


	public class matching_timeout {
	
		public class request : SprotoTypeBase {
			private static int max_field_count = 0;
			
			
			public request () : base(max_field_count) {}

			public request (byte[] buffer) : base(max_field_count, buffer) {
				this.decode ();
			}

			protected override void decode () {
				int tag = -1;
				while (-1 != (tag = base.deserialize.read_tag ())) {
					switch (tag) {
					default:
						base.deserialize.read_unknow_data ();
						break;
					}
				}
			}

			public override int encode (SprotoStream stream) {
				base.serialize.open (stream);

				return base.serialize.close ();
			}
		}


	}


	public class package : SprotoTypeBase {
		private static int max_field_count = 3;
		
		
		private Int64 _type; // tag 0
		public Int64 type {
			get { return _type; }
			set { base.has_field.set_field (0, true); _type = value; }
		}
		public bool HasType {
			get { return base.has_field.has_field (0); }
		}

		private Int64 _session; // tag 1
		public Int64 session {
			get { return _session; }
			set { base.has_field.set_field (1, true); _session = value; }
		}
		public bool HasSession {
			get { return base.has_field.has_field (1); }
		}

		private string _ud; // tag 2
		public string ud {
			get { return _ud; }
			set { base.has_field.set_field (2, true); _ud = value; }
		}
		public bool HasUd {
			get { return base.has_field.has_field (2); }
		}

		public package () : base(max_field_count) {}

		public package (byte[] buffer) : base(max_field_count, buffer) {
			this.decode ();
		}

		protected override void decode () {
			int tag = -1;
			while (-1 != (tag = base.deserialize.read_tag ())) {
				switch (tag) {
				case 0:
					this.type = base.deserialize.read_integer ();
					break;
				case 1:
					this.session = base.deserialize.read_integer ();
					break;
				case 2:
					this.ud = base.deserialize.read_string ();
					break;
				default:
					base.deserialize.read_unknow_data ();
					break;
				}
			}
		}

		public override int encode (SprotoStream stream) {
			base.serialize.open (stream);

			if (base.has_field.has_field (0)) {
				base.serialize.write_integer (this.type, 0);
			}

			if (base.has_field.has_field (1)) {
				base.serialize.write_integer (this.session, 1);
			}

			if (base.has_field.has_field (2)) {
				base.serialize.write_string (this.ud, 2);
			}

			return base.serialize.close ();
		}
	}


	public class player_info : SprotoTypeBase {
		private static int max_field_count = 3;
		
		
		private string _rid; // tag 0
		public string rid {
			get { return _rid; }
			set { base.has_field.set_field (0, true); _rid = value; }
		}
		public bool HasRid {
			get { return base.has_field.has_field (0); }
		}

		private string _rname; // tag 1
		public string rname {
			get { return _rname; }
			set { base.has_field.set_field (1, true); _rname = value; }
		}
		public bool HasRname {
			get { return base.has_field.has_field (1); }
		}

		private Int64 _side; // tag 2
		public Int64 side {
			get { return _side; }
			set { base.has_field.set_field (2, true); _side = value; }
		}
		public bool HasSide {
			get { return base.has_field.has_field (2); }
		}

		public player_info () : base(max_field_count) {}

		public player_info (byte[] buffer) : base(max_field_count, buffer) {
			this.decode ();
		}

		protected override void decode () {
			int tag = -1;
			while (-1 != (tag = base.deserialize.read_tag ())) {
				switch (tag) {
				case 0:
					this.rid = base.deserialize.read_string ();
					break;
				case 1:
					this.rname = base.deserialize.read_string ();
					break;
				case 2:
					this.side = base.deserialize.read_integer ();
					break;
				default:
					base.deserialize.read_unknow_data ();
					break;
				}
			}
		}

		public override int encode (SprotoStream stream) {
			base.serialize.open (stream);

			if (base.has_field.has_field (0)) {
				base.serialize.write_string (this.rid, 0);
			}

			if (base.has_field.has_field (1)) {
				base.serialize.write_string (this.rname, 1);
			}

			if (base.has_field.has_field (2)) {
				base.serialize.write_integer (this.side, 2);
			}

			return base.serialize.close ();
		}
	}


}


public class S2C_Protocol : ProtocolBase {
	public static  S2C_Protocol Instance = new S2C_Protocol();
	private S2C_Protocol() {
		Protocol.SetProtocol<error> (error.Tag);
		Protocol.SetRequest<S2C_SprotoType.error.request> (error.Tag);

		Protocol.SetProtocol<game_block_buffer> (game_block_buffer.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_block_buffer.request> (game_block_buffer.Tag);

		Protocol.SetProtocol<game_garbage_buffer> (game_garbage_buffer.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_garbage_buffer.request> (game_garbage_buffer.Tag);

		Protocol.SetProtocol<game_info> (game_info.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_info.request> (game_info.Tag);

		Protocol.SetProtocol<game_matched> (game_matched.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_matched.request> (game_matched.Tag);

		Protocol.SetProtocol<game_new_row> (game_new_row.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_new_row.request> (game_new_row.Tag);

		Protocol.SetProtocol<game_over> (game_over.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_over.request> (game_over.Tag);

		Protocol.SetProtocol<game_raise> (game_raise.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_raise.request> (game_raise.Tag);

		Protocol.SetProtocol<game_ready> (game_ready.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_ready.request> (game_ready.Tag);

		Protocol.SetProtocol<game_rollback> (game_rollback.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_rollback.request> (game_rollback.Tag);

		Protocol.SetProtocol<game_start> (game_start.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_start.request> (game_start.Tag);

		Protocol.SetProtocol<game_swap> (game_swap.Tag);
		Protocol.SetRequest<S2C_SprotoType.game_swap.request> (game_swap.Tag);

		Protocol.SetProtocol<matching_error> (matching_error.Tag);
		Protocol.SetRequest<S2C_SprotoType.matching_error.request> (matching_error.Tag);

		Protocol.SetProtocol<matching_success> (matching_success.Tag);
		Protocol.SetRequest<S2C_SprotoType.matching_success.request> (matching_success.Tag);

		Protocol.SetProtocol<matching_timeout> (matching_timeout.Tag);
		Protocol.SetRequest<S2C_SprotoType.matching_timeout.request> (matching_timeout.Tag);

	}

	public class error {
		public const int Tag = 5001;
	}

	public class game_block_buffer {
		public const int Tag = 5069;
	}

	public class game_garbage_buffer {
		public const int Tag = 5070;
	}

	public class game_info {
		public const int Tag = 5060;
	}

	public class game_matched {
		public const int Tag = 5067;
	}

	public class game_new_row {
		public const int Tag = 5068;
	}

	public class game_over {
		public const int Tag = 5080;
	}

	public class game_raise {
		public const int Tag = 5066;
	}

	public class game_ready {
		public const int Tag = 5061;
	}

	public class game_rollback {
		public const int Tag = 5075;
	}

	public class game_start {
		public const int Tag = 5062;
	}

	public class game_swap {
		public const int Tag = 5065;
	}

	public class matching_error {
		public const int Tag = 5042;
	}

	public class matching_success {
		public const int Tag = 5040;
	}

	public class matching_timeout {
		public const int Tag = 5041;
	}

}