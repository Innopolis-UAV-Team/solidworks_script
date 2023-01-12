using sw_exporter.Pipeline.Messages.Base;

namespace sw_exporter.Pipeline.Stages.Base {
    public interface IStage {
        Message Process(Message data);
    }
}