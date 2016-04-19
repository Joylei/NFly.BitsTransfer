# NFly.BitsTransfer
A .Net wrapper library for Windows [Background Intelligent Transfer Service (BITS) ](https://msdn.microsoft.com/en-us/library/aa362708%28v=vs.85%29.aspx)

# DEMO
![DEMO](https://github.com/Joylei/NFly.BitsTransfer/blob/master/Capture.PNG)

# Features
- Download Single File
- Download Folder
- Progress report
- Estimated downloading time
- Estimated downloading speed

# Usage
```cs
var jobName = "YOUR_JOB_NAME"
var job = BitsJob.GetJob(jobName);

//remove existing job
if(job!=null){
  job.Cancel();  
}

//create job
job = BitsJob.Create(jobName);
job.Pause();
job.AddFile("REMOTE_FILE", "PATH_OF_LOCAL_FILE");

//watch progress
job.OnProgress += progress =>{
  var status = job.State;
  if(status == BitsJob.BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED){
    //files transferred, mark job as completed
    job.Complete();

    Console.WriteLine("Job completed, you can access downloaded files now.");
    return;
  }

  Console.WriteLine("----");
  Console.WriteLine("BytesTotal: {0}",progress.BytesTotal);
  Console.WriteLine("BytesTransferred: {0}",progress.BytesTransferred);
  Console.WriteLine("Bytes Per Second: {0}",progress.EstimatedSpeed);
  Console.WriteLine("Remaining Seconds: {0}",progress.EstimatedTimeRemaining);
};

//pause job
job.Pause();

//resume job
job.Resume();
```
