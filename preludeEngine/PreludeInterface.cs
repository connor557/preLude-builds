/*
 * Created by SharpDevelop.
 * User: novalis78
 * Date: 05.12.2004
 * Time: 11:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SpeechLib;
using System.Threading;
using System.Timers;
using NLog;

namespace PreludeEngine
{
	/// <summary>
	/// TODO: check speech engine -- if too fast it stops
	/// that never happens when using msagent speechlib
	/// 2.) incorporate speechrecognition
	/// </summary>
	public class PreLudeInterface
	{
		private Mind mindInstance = null;	
		public string loadedMind 	= "mind.mdu";
		public bool isContributable = false;
		public bool isSpeaking      = false;
		public bool proactiveMode   = true;
        public bool quantumRandomness = false;
		private int idleTime 		= 0;
		private string autoSpeakInput 		= "";
		private System.Timers.Timer timer 	= null;
		public delegate string AutoSpeakHandler(string boredString);
		public AutoSpeakHandler reportBoredom;
        public DateTime ChatInitiated;
        public PreludeEngine.Mind.MatchingAlgorithm initializedAssociater = Mind.MatchingAlgorithm.Basic;
        private static Logger logger = LogManager.GetCurrentClassLogger();
		
		public void initializeEngine()
		{
			mindInstance = new Mind(loadedMind, false);
			mindInstance.analyzeShortTermMemory();
            mindInstance.associater = initializedAssociater;

            ChatInitiated = DateTime.Now;

			if(proactiveMode)
			{
				timer = new System.Timers.Timer();
				timer.Elapsed += new System.Timers.ElapsedEventHandler(autoAnswering);
			}
		}
		
		public string chatWithPrelude(string input)
		{
			if(mindInstance == null) return "Error: Mind not initialized";
			if(proactiveMode)
			{
				idleTime = 0;
				if(timer != null)
					timer.Stop();
			}
            if (quantumRandomness)
                mindInstance.quantumRandomness = true;

            if (avoidLearnByRepeating)
                mindInstance.avoidLearnByRepeating = true;

			string output = "";
			output = mindInstance.listenToInput(input);
			if(isSpeaking)
				speak(output);
			if(proactiveMode)
			{
                SetTimer();
				autoSpeakInput = output;
				timer.Start();
			}
			return output;	
		}

        /// <summary>
        /// sets the interval so as to make prelude appear to be 
        /// trying to get the user back into a conversation...
        /// </summary>
        private void SetTimer()
        {
            DateTime n = DateTime.Now;
            TimeSpan t = n - ChatInitiated;
            double y = ((Convert.ToInt64(t.TotalSeconds) ^ 2) * 1000) + 5000;
            double x = t.TotalSeconds * 1000;
            Random random = new Random();
            idleTime = random.Next(Convert.ToInt32(x), Convert.ToInt32(y));
            timer.Interval = idleTime;
            if (x > 500000)
                ChatInitiated = DateTime.Now;

            logger.Trace("Boredom time function: " + " x: " + x + " y: " + y);
        }
		
		public void autoAnswering(object sender, System.Timers.ElapsedEventArgs e)
		{
            try
            {
                //trigger auto answer to frontend
                if (timer.Enabled != false)
                {
                    string t = mindInstance.listenToInput(autoSpeakInput);
                    Console.WriteLine("You: (away)");
                    Console.WriteLine("Prelude bored: " + t);
                    SetTimer();
                }
            }
            catch (System.Exception ex)
            {
                //logger...
                ;
            }
		}
	
		
		public void speak(string a)
		{
			if(mindInstance == null) return;
			try
            {
            	SpeechVoiceSpeakFlags SpFlags = SpeechVoiceSpeakFlags.SVSFlagsAsync;
                SpVoice speech = new SpVoice();
                if (isSpeaking)
                {
                    speech.Speak(a, SpFlags);    
                }
        	}
        	catch
            {
            	;
            }
		}
		
		public void stopPreludeEngine()
		{
			if(mindInstance == null) return;
			mindInstance.prepareCurrentMemoryForDisc();
			if(isContributable)
			   mindInstance.contributeClientMind();
		}
		
		public void forcedContribution()
		{	
			if(mindInstance == null) return;
			if(isContributable)
			   mindInstance.contributeClientMind();
		}
		//save current mind to disc
		public void forcedSaveMindFile()
		{
			if(mindInstance == null) return;
			mindInstance.prepareCurrentMemoryForDisc();
		}
		
		//save current mind to disc there is another way too!
		public void forcedSaveMindFile(string a)
		{
			if(mindInstance == null) return;
			mindInstance.prepareCurrentMemoryForDisc(a);
		}		
		
		//count currently loaded bot memory
		public int countMindMemory()
		{
			if(mindInstance == null) return -1;
			int i = 0;
			i = mindInstance.memorySize;
			return i;
		}
		
		public string getVersionInfo()
		{
			return "Prelude@# Engine, version 1.2.7, 2004-2015(c) by Lennart Lopin ";
		}
		
		public bool setPreludeClient(string server, int port)
		{
			if(mindInstance != null) return false;
			PreLudeClient.port = port;
			PreLudeClient.server = server;
			return true;
		}
		
		public bool setProactiveMode(bool a)
		{
            if (mindInstance == null) return false;
			mindInstance.proactiveMode = a;
			return false;
		}

        public bool avoidLearnByRepeating { get; set; }
    }
}
