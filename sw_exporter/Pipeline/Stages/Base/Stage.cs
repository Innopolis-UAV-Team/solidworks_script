using System;
using sw_exporter.Pipeline.Messages.Base;

namespace sw_exporter.Pipeline.Stages.Base {
    
    public class Stage : IStage {
        private Stage _next;
        private Stage _head;

        public Stage() {
            _head = this;
        }

        public virtual Message Process(Message data) {
            throw new System.NotImplementedException();
        }

        public Stage Register(Stage stage) {
            _next = stage;
            stage._head = _head;
            return stage;
        }

        public Message Run(Message input) {
            var nextNode = _head;
            var result = input;
            while (nextNode != null) {
                Console.WriteLine($"Next {nextNode}");
                result = nextNode.Process(result);
                nextNode = nextNode._next;
            }

            return result;
        }
    }
    
} 