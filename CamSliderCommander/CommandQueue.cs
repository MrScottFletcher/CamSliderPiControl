using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public class Command
    {
        public DateTime AddedDateTime { get; set; }
        public string Source { get; set; }
        public string Description { get; set; }
        public string ASCIItoSend { get; set; }
        public DateTime? Sent { get; set; }
        public string ReturnMessage { get; set; }
    }
    public class CommandQueue
    {


        static System.Threading.ReaderWriterLock _lock;
        private const int _writerLockTimeoutMs = 10000;

        private List<Command> _commands;

        public delegate void QueuChangedEventHandler(object sender);
        public event QueuChangedEventHandler QueueChanged;


        public CommandQueue()
        {
            _lock = new System.Threading.ReaderWriterLock();
            _commands = new List<Command>();
        }

        public Command GetNextCommandToSend()
        {
            Command ret = DoActionWithReaderLock(() =>
            {
                var notSentList = _commands.Where(n => !n.Sent.HasValue);
                if (notSentList.Count() > 0) return notSentList.First();
                else return null;
            });

            return ret;
        }

        public Command GetFinalCommandToSend()
        {
            Command ret = DoActionWithReaderLock(() =>
            {
                var notSentList = _commands.Where(n => !n.Sent.HasValue);
                if (notSentList.Count() > 0) return notSentList.Last();
                else return null;
            });

            return ret;
        }

        public List<Command> GetCommandList()
        {
            List<Command> ret = DoActionWithReaderLock(() =>
            {
                return _commands.ToList();
            });

            return ret;
        }


        public void ClearAllCommands()
        {
            DoActionWithWriterLock(() => _commands.Clear());
        }

        public void AddCommand(Command command)
        {
            DoActionWithWriterLock(() => _commands.Add(command));
        }
        public void AddCommand(string source, string description, string ASCIItoSend)
        {
            DoActionWithWriterLock(() => _commands.Add(new Command() { Source = source, Description = description, ASCIItoSend = ASCIItoSend }));
        }

        public void RemoveCommand(Command command)
        {
            DoActionWithWriterLock(() => _commands.Remove(command));
        }
        public void RemoveAllCommandsForSource(string source)
        {
            DoActionWithWriterLock(() => _commands.RemoveAll(s => s.Source == source));
        }
        public void MarkCommandSent(Command command)
        {
            DoActionWithWriterLock(() =>
            {
                Command found = null;
                int index = _commands.IndexOf(command);
                if (index >= 0) found = _commands[index];
                if (found != null)
                    found.Sent = DateTime.Now;
            });
        }


        public static void DoActionWithWriterLock(Action funcToRun)
        {
            bool hadReaderLock = false;
            bool hadWriterLock = false;
            LockCookie cookie;

            if (!_lock.IsWriterLockHeld)
            {
                if (_lock.IsReaderLockHeld)
                {
                    hadReaderLock = true;
                    cookie = _lock.UpgradeToWriterLock(_writerLockTimeoutMs);
                }

                if (!_lock.IsWriterLockHeld)
                    _lock.AcquireWriterLock(_writerLockTimeoutMs);
            }
            else
            {
                hadWriterLock = true;
            }

            if (!_lock.IsWriterLockHeld)
            {
                throw new ApplicationException("Unable to acquire WRITER lock on CommandQueue after waiting " + _writerLockTimeoutMs + " milliseconds.");
            }

            //do the thing
            funcToRun();

            if (hadWriterLock)
            {
                //keep it
            }
            else if (hadReaderLock && _lock.IsWriterLockHeld)
            {
                //Reset the 
                _lock.DowngradeFromWriterLock(ref cookie);
            }
            else
            {
                //release it all
                _lock.ReleaseWriterLock();
            }
        }

        public static Command DoActionWithReaderLock(Func<Command> funcToRun)
        {
            Command found = null;
            bool hadLock = false;

            if (_lock.IsWriterLockHeld || _lock.IsReaderLockHeld)
            {
                //use it;
                hadLock = true;
            }
            else
            {
                _lock.AcquireReaderLock(_writerLockTimeoutMs);
            }

            if (!_lock.IsReaderLockHeld)
            {
                throw new ApplicationException("Unable to acquire Reader lock on CommandQueue (to return single) after waiting " + _writerLockTimeoutMs + " milliseconds.");
            }

            //do the thing
            found = funcToRun();

            if (hadLock)
            {
                //keep it
            }
            else
            {
                //Reset the 
                _lock.ReleaseReaderLock();
            }
            return found;
        }
        public static List<Command> DoActionWithReaderLock(Func<List<Command>> funcToRun)
        {
            List<Command> found = null;

            bool hadLock = false;

            if (_lock.IsWriterLockHeld || _lock.IsReaderLockHeld)
            {
                //use it;
                hadLock = true;
            }
            else
            {
                _lock.AcquireReaderLock(_writerLockTimeoutMs);
            }

            if (!_lock.IsReaderLockHeld)
            {
                throw new ApplicationException("Unable to acquire Reader lock on CommandQueue (to return list) after waiting " + _writerLockTimeoutMs + " milliseconds.");
            }

            //do the thing
            found = funcToRun();

            if (hadLock)
            {
                //keep it
            }
            else
            {
                //Reset the 
                _lock.ReleaseReaderLock();
            }
            return found;
        }


        protected void FireQueueChanged()
        {
            QueueChanged?.Invoke(this);
        }


    }
}
