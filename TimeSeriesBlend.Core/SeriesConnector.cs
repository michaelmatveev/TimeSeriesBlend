using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TimeSeriesBlend.Core.Grammar;
using TimeSeriesBlend.Core.MetaVariables;
using TimeSeriesBlend.Core.Periods;

namespace TimeSeriesBlend.Core
{
    public sealed class SeriesConnector<H> : 
        IDefineGroup, 
        IDefinePeriod,
        IPeriodVariables,           // should be implemented explicitly
        IEndGroupOrDefinePeriod,    // should be implemented explicitly
        IPeriodVariableAssigment,   // should be implemented explicitly
        IPeriodVariableReader,      // should be implemented explicitly
        IComputable                 // should be implemented explicitly     
    {
        #region Initialization 

        public const string RootGroupName = "{60E7492A-9191-4A4D-B4CB-D36E5C6F2EFF}";

        /// <summary>
        /// Объект для хранения промежуточных значений
        /// </summary>
        private H _propertyHolder;

        public SeriesConnector(H holder)
        {
            _propertyHolder = holder;
            _currentPeriod = _constantPeriod = new ConstantPeriod();
            _periodHolders.Add(_constantPeriod);

            var defaultGroup = new GroupOfVariables
            {
                Name = RootGroupName,
                MembersGenerator = (o) => new[] { o }
            };

            _Groups.Add(defaultGroup);
            _currentGroup = defaultGroup;
        }

        #endregion

        #region Period definitions

        private ConstantPeriod _constantPeriod;
        private CalculationPeriod _currentPeriod;
        private readonly List<CalculationPeriod> _periodHolders = new List<CalculationPeriod>();

        public IPeriodVariables BeginPeriod(Func<DateTime, DateTime> getNextPeriod)
        {
            return (this as IEndGroupOrDefinePeriod).BeginPeriod(getNextPeriod);
        }

        public IPeriodVariables BeginPeriod(string caption, Func<DateTime, DateTime> getNextPeriod)
        {
            return (this as IEndGroupOrDefinePeriod).BeginPeriod(caption, getNextPeriod);
        }

        public IPeriodVariables InPeriod(string caption)
        {
            return (this as IEndGroupOrDefinePeriod).InPeriod(caption);
        }

        public IPeriodVariables InConstants()
        {
            return (this as IEndGroupOrDefinePeriod).InConstants();
        } 

        IPeriodVariables IDefinePeriod.BeginPeriod(Func<DateTime, DateTime> getNextPeriod)
        {
            return (this as IEndGroupOrDefinePeriod).BeginPeriod(null, getNextPeriod);
        }

        IPeriodVariables IDefinePeriod.BeginPeriod(string caption, Func<DateTime, DateTime> getNextPeriod)
        {
            _currentPeriod = _periodHolders
                .Where(p => p.WithinGroup == _currentGroup)
                .SingleOrDefault(p =>
                    (p.Name != null && p.Name == caption)
                    ||
                    (p.Name == null && p.GetNextPeriod == getNextPeriod));

            if (_currentPeriod == null)
            {
                _currentPeriod = new CalculationPeriod
                {
                    Name = caption,
                    GetNextPeriod = getNextPeriod,
                    WithinGroup = _currentGroup
                };

                _periodHolders.Add(_currentPeriod);
            }
            return this;
        }

        IPeriodVariables IDefinePeriod.InPeriod(string caption)
        {
            _currentPeriod = _periodHolders
               .Where(p => p.WithinGroup == _currentGroup)
               .Single(p => p.Name == caption);

            return this;
        }

        IPeriodVariables IDefinePeriod.InConstants()
        {
            _currentPeriod = _constantPeriod;
            return this;
        }

        IEndGroupOrDefinePeriod IPeriodVariables.EndPeriod()
        {
            return this;
        }

        IDefineGroup IEndGroupOrDefinePeriod.EndGroup()
        {
            return this;
        }

        #endregion

        #region Groups

        private GroupOfVariables _currentGroup;
        private readonly IList<GroupOfVariables> _Groups = new List<GroupOfVariables>();

        public IDefinePeriod BeginGroup(string caption, Func<object, IEnumerable> members)
        {
            var newGroup = new GroupOfVariables
            {
                Name = caption,
                MembersGenerator = members,
                Parent = _currentGroup
            };

            _currentGroup.Children.Add(newGroup);
            _currentGroup = newGroup;
            _Groups.Add(_currentGroup);

            return this;
        }

        public IDefinePeriod BeginGroup(string caption, Func<IEnumerable> members)
        {
            var newGroup = new GroupOfVariables
            {
                Name = caption,
                MembersGenerator = (o) => members(),
                Parent = _currentGroup
            };

            _currentGroup.Children.Add(newGroup);
            _currentGroup = newGroup;
            _Groups.Add(_currentGroup);

            return this;
        }

        #endregion

        #region Variables

        private string _currentVariableName;
        private PropertyInfo _currentPropertyInfo;
        private IList<MetaVariable<H>> _variables = new List<MetaVariable<H>>();

        /// <summary>
        /// Объявляет простую переменную
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metaVar"></param>
        /// <returns></returns>
        IPeriodVariableAssigment IPeriodVariables.Let<T>(Expression<Func<T>> metaVar)
        {            
            return (this as IPeriodVariables).Let(string.Empty, metaVar);
        }

        IPeriodVariableAssigment IPeriodVariables.Let<T>(string name, Expression<Func<T>> metaVar)
        {
            var me = metaVar.Body as MemberExpression;
            _currentPropertyInfo = me.Member as PropertyInfo;
            _currentVariableName = string.IsNullOrEmpty(name) ? _currentPropertyInfo.Name : name;

            return this;
        }

        #endregion

        #region Sliced variable declaring

        IPeriodVariableAssigment IPeriodVariables.Let<T>(Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn)
        {
            return (this as IPeriodVariables).Let(string.Empty, metaVar, basedOn);
        }

        /// <summary>
        /// Объявляет переменную в виде списка, занося в нее значения из другой переменной за соответсвующий период
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="metaVar"></param>
        /// <param name="basedOn"></param>
        /// <returns></returns>
        IPeriodVariableAssigment IPeriodVariables.Let<T>(string name, Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn)
        {
            (this as IPeriodVariables).Let(name, metaVar);

            MemberExpression me = basedOn.Body as MemberExpression;
            PropertyInfo baseProperty = me.Member as PropertyInfo;

            _variables.Add(new SlicedVariable<H, T>
            {
                Period = _currentPeriod,
                Group = _currentGroup,
                MetaProperty = _currentPropertyInfo,
                BasedOnMetaProperty = baseProperty,
                Name = _currentVariableName
            });

            return this;
        }

        #endregion

        #region Cross group variable declaring

        IPeriodVariableAssigment IPeriodVariables.Let<K, T>(Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn)
        {            
            return (this as IPeriodVariables).Let(String.Empty, metaVar, basedOn);
        }

        IPeriodVariableAssigment IPeriodVariables.Let<K, T>(string name, Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn)
        {
            (this as IPeriodVariables).Let(name, metaVar);

            MemberExpression me = basedOn.Body as MemberExpression;
            PropertyInfo baseProperty = me.Member as PropertyInfo;

            _variables.Add(new CrossGroupVariable<H, K, T>
            {
                Period = _currentPeriod,
                Group = _currentGroup,
                MetaProperty = _currentPropertyInfo,
                BasedOnMetaProperty = baseProperty,
                Name = _currentVariableName
            });

            return this;
        }

        #endregion

        #region Shifted vars assign

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<T>> basedOn, int shift)
        {
            Func<TimeArg, T> wrap = tp => default(T);
            return (this as IPeriodVariableAssigment).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<T> emptyFiller)
        {
            Func<TimeArg, T> wrap = tp => emptyFiller();
            return (this as IPeriodVariableAssigment).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<DateTime, T> emptyFiller)
        {
            Func<TimeArg, T> wrap = tp => emptyFiller(tp.T);
            return (this as IPeriodVariableAssigment).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<DateTime, Int32, T> emptyFiller)
        {
            Func<TimeArg, T> wrap = tp => emptyFiller(tp.T, tp.I);
            return (this as IPeriodVariableAssigment).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<TimeArg, T> emptyFiller)
        {
            MemberExpression me = basedOn.Body as MemberExpression;
            PropertyInfo baseProperty = me.Member as PropertyInfo;

            var variable = new ShiftedVaraible<H, T>
            {
                Period = _currentPeriod,
                Group = _currentGroup,
                MetaProperty = _currentPropertyInfo,
                Name = _currentVariableName ?? _currentPropertyInfo.Name,
                BasedOnMetaProperty = baseProperty,
                Shift = shift,
                EmptyFiller = emptyFiller
            };

            _variables.Add(variable);
            _currentVariableName = null;
            _currentPropertyInfo = null;

            return this;
        }

        #endregion
        
        #region Calculated value assign

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<T>> writer)
        {
            return (this as IPeriodVariableAssigment).Assign<T>(ExpressionsBuilder.ConvertExpression<T>(writer));
        }

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<DateTime, T>> writer)
        {
            return (this as IPeriodVariableAssigment).Assign<T>(ExpressionsBuilder.ConvertExpression<T>(writer));
        }

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<DateTime, int, T>> writer)
        {
            return (this as IPeriodVariableAssigment).Assign<T>(ExpressionsBuilder.ConvertExpression<T>(writer));
        }

        IPeriodVariableReader IPeriodVariableAssigment.Assign<T>(Expression<Func<TimeArg, T>> writer)
        {
            var variable = new CalculatedVariable<H, T>
            {
                Period = _currentPeriod,
                Group = _currentGroup,
                MetaProperty = _currentPropertyInfo,
                Name = _currentVariableName ?? _currentPropertyInfo.Name,
                Writer = writer
            };

            _variables.Add(variable);
            _currentVariableName = null;
            _currentPropertyInfo = null;

            return this;
        }

        #endregion

        #region Read data after calc

        IPeriodVariableReader IPeriodVariableReader.Read(Action reader)
        {
            return (this as IPeriodVariableReader).Read((TimeArg tp) => reader());
        }

        IPeriodVariableReader IPeriodVariableReader.Read(Action<DateTime> reader)
        {
            return (this as IPeriodVariableReader).Read(tp => reader(tp.T));
        }

        IPeriodVariableReader IPeriodVariableReader.Read(Action<DateTime, int> reader)
        {
            return (this as IPeriodVariableReader).Read(tp => reader(tp.T, tp.I));
        }

        IPeriodVariableReader IPeriodVariableReader.Read(Action<TimeArg> reader)
        {
            _variables.Last().Readers.Add(reader);
            return this;
        }

        IPeriodVariables IPeriodVariableReader.End()
        {
            return this;
        }

        #endregion

        #region Summary

        IEndGroupOrDefinePeriod IPeriodVariables.Summarize(Action<DateTime> action)
        {
            return (this as IPeriodVariables).Summarize(tp => action(tp.T));
        }

        IEndGroupOrDefinePeriod IPeriodVariables.Summarize(Action<DateTime, Int32> action)
        {
            return (this as IPeriodVariables).Summarize(tp => action(tp.T, tp.I));
        }

        IEndGroupOrDefinePeriod IPeriodVariables.Summarize(Action<TimeArg> action)
        {
            _variables.Add(new SummarizeVariable<H>(action)
            {
                Name = $"summary for {_currentPeriod.Name ?? "unnamed"} period",
                Period = _currentPeriod,
                Group = _currentGroup,
            });

            return this;
        }

        #endregion

        #region Compilation

        public static IComputable Compile(SeriesConnector<H> source)
        {
            foreach (MetaVariable<H> v in source._variables)
            {
                v.Compile(source._variables);
            }
            return source;
        }

        #endregion

        #region Execution

        void IComputable.Compute(ComputationParameters parameters)
        {
            IEnumerable<MetaVariable<H>> varsToCompute;
            if (parameters.VariablesToCompute != null)
            {
                varsToCompute = _variables.Where(v => parameters.VariablesToCompute.Contains(v.Name));
            }
            else
            {
                varsToCompute = _variables;
            }

            if (!varsToCompute.All(v => v.State == CompilationState.Compiled))
            {
                throw new InvalidOperationException(@"Method \""Compile\"" must be called before computing");
            }

            // сначала помечаем все переменные как ненужные для вычислений
            foreach (var v in _variables)
            {
                v.RequiredCalculation = false;
            }

            // затем выставляем только те, которые нужно вычислить в зависимости от
            // выбранных переменных, зависимости у выбранных переменных и наличия ридеров
            MarkVariablesToCompute(varsToCompute.Where(r => r.Readers.Any()));

            if (MarkedVariables.Any())
            {
                foreach (var p in _periodHolders)
                {
                    p.GeneratePeriods(parameters.From, parameters.Till);
                }

                // вычисляем только те строки из которых в реальности необходимо получить данные
                Execute();
            }
        }

        event EventHandler<ProgressArgs> IComputable.OnProgress
        {
            add
            {
                lock(OnProgress)
                {
                    OnProgress += value;
                }
            }

            remove
            {
                lock (OnProgress)
                {
                    OnProgress -= value;
                }
            }
        }

        event EventHandler<ComputationArgs> IComputable.OnComputationStart
        {
            add
            {
                lock(OnComputationStart)
                {
                    OnComputationStart += value;
                }
            }

            remove
            {
                lock(OnComputationStart)
                {
                    OnComputationStart -= value;
                }
            }
        }

        event EventHandler<ComputationArgs> IComputable.OnComputationFinish
        {
            add
            {
                lock(OnComputationFinish)
                {
                    OnComputationFinish += value;
                }
            }

            remove
            {
                lock(OnComputationFinish)
                {
                    OnComputationFinish -= value;
                }
            }
        }

        event EventHandler<ComputationErrorArgs> IComputable.OnComputationError
        {
            add
            {
                lock(OnComputationError)
                {
                    OnComputationError += value;
                }
            }

            remove
            {
                lock(OnComputationError)
                {
                    OnComputationError -= value;
                }
            }
        }

        private int _runCount;

        event EventHandler<ProgressArgs> OnProgress;
        private bool OnProgressHandler(String varName, String groupName, Int32 stepsCount, Int32 current)
        {
            if (OnProgress != null)
            {
                var args = new ProgressArgs
                {
                    VariableName = varName,
                    NumberOfAttempts = _runCount,
                    Current = current,
                    UpperBound = stepsCount,
                    GroupName = groupName
                };

                OnProgress(this, args);

                return args.Cancel;
            }

            return false;
        }

        event EventHandler<ComputationArgs> OnComputationStart;
        private void OnComputationStartHandler()
        {
            OnComputationStart?.Invoke(this, new ComputationArgs
            {
                NumberOfAttempts = _runCount
            });
        }

        event EventHandler<ComputationArgs> OnComputationFinish;
        private void OnComputationFinishHandler()
        {
            OnComputationFinish?.Invoke(this, new ComputationArgs
            {
                NumberOfAttempts = _runCount
            });
        }

        event EventHandler<ComputationErrorArgs> OnComputationError;
        private void OnComputationErrorHandler(Exception ex)
        {
            OnComputationError?.Invoke(this, new ComputationErrorArgs
            {
                NumberOfAttempts = _runCount,
                Exception = ex
            });
        }

        private void MarkVariablesToCompute(IEnumerable<MetaVariable<H>> input)
        {
            foreach (var v in input)
            {
                v.RequiredCalculation = true;
                MarkVariablesToCompute(v.DependsOn);
            }
        }

        private IEnumerable<MetaVariable<H>> MarkedVariables
        {
            get
            {
                return _variables.Where(v => v.RequiredCalculation);
            }
        }

        private void Execute()
        {
            try
            {
                OnComputationStartHandler();

                var groupAndVariables = _Groups
                    .GroupJoin(
                        MarkedVariables,
                        g => g,
                        v => v.Group,
                        (g, vs) => new
                        {
                            Group = g,
                            VariablesInGroup = vs
                        });

                foreach (var gr in groupAndVariables.Select(gv => gv.Group).Where(g => g.GetLevel() == 0))
                {
                    gr.GetMembers(new MemberInfo
                    {
                        Value = new object(),
                        ParentMember = null
                    }); // this will recursive call
                }

                int upperBound = groupAndVariables.Sum(gv => gv.VariablesInGroup.Count() * gv.Group.Members.Count());
                int current = 0;
                foreach (var group in _Groups.OrderByDescending(g => g.GetLevel())) // начинаем с самых вложенных групп
                {
                    var variablesInGroup = MarkedVariables.Where(v => v.Group == group);

                    foreach (var member in group.Members)
                    {
                        var moniker = Guid.NewGuid(); // moniker генерируется для каждого нового объекта из группы

                        foreach (MetaVariable<H> v in variablesInGroup)
                        {
                            v.Evaluate(_propertyHolder, member, moniker);
                            if (OnProgressHandler(v.Name, group.Name, upperBound, ++current))
                            {
                                goto End_Of_Computation;
                            }
                        }
                    }
                }

            End_Of_Computation:
                OnComputationFinishHandler();
            }
            catch (Exception ex)
            {
                OnComputationErrorHandler(ex);
            }
            finally
            {
                _runCount++;
            }
        }

        #endregion
    }
}
