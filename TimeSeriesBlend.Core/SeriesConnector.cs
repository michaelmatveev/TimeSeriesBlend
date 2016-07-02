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
    public class SeriesConnector<H, I> : 
        IDefineGroup<I>, 
        IDefinePeriod<I>,
        IPeriodVariables<I>,           // should be implemented explicitly
        IEndGroupOrDefinePeriod<I>,    // should be implemented explicitly
        IPeriodVariableAssigment<I>,   // should be implemented explicitly
        IPeriodVariableReader<I>,      // should be implemented explicitly
        IComputable<I>                 // should be implemented explicitly     
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
            _currentPeriod = _constantPeriod = new ConstantPeriod<I>();
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

        private ConstantPeriod<I> _constantPeriod;
        private CalculationPeriod<I> _currentPeriod;
        private readonly List<CalculationPeriod<I>> _periodHolders = new List<CalculationPeriod<I>>();

        public IPeriodVariables<I> BeginPeriod(Func<I, I> getNextPeriod)
        {
            return (this as IEndGroupOrDefinePeriod<I>).BeginPeriod(getNextPeriod);
        }

        public IPeriodVariables<I> BeginPeriod(string caption, Func<I, I> getNextPeriod)
        {
            return (this as IEndGroupOrDefinePeriod<I>).BeginPeriod(caption, getNextPeriod);
        }

        public IPeriodVariables<I> InPeriod(string caption)
        {
            return (this as IEndGroupOrDefinePeriod<I>).InPeriod(caption);
        }

        public IPeriodVariables<I> InConstants()
        {
            return (this as IEndGroupOrDefinePeriod<I>).InConstants();
        } 

        IPeriodVariables<I> IDefinePeriod<I>.BeginPeriod(Func<I, I> getNextPeriod)
        {
            return (this as IEndGroupOrDefinePeriod<I>).BeginPeriod(null, getNextPeriod);
        }

        IPeriodVariables<I> IDefinePeriod<I>.BeginPeriod(string caption, Func<I, I> getNextPeriod)
        {
            _currentPeriod = _periodHolders
                .Where(p => p.WithinGroup == _currentGroup)
                .SingleOrDefault(p =>
                    (p.Name != null && p.Name == caption)
                    ||
                    (p.Name == null && p.GetNextPeriod == getNextPeriod));

            if (_currentPeriod == null)
            {
                _currentPeriod = new CalculationPeriod<I>
                {
                    Name = caption,
                    GetNextPeriod = getNextPeriod,
                    WithinGroup = _currentGroup
                };

                _periodHolders.Add(_currentPeriod);
            }
            return this;
        }

        IPeriodVariables<I> IDefinePeriod<I>.InPeriod(string caption)
        {
            _currentPeriod = _periodHolders
               .Where(p => p.WithinGroup == _currentGroup)
               .Single(p => p.Name == caption);

            return this;
        }

        IPeriodVariables<I> IDefinePeriod<I>.InConstants()
        {
            _currentPeriod = _constantPeriod;
            return this;
        }

        IEndGroupOrDefinePeriod<I> IPeriodVariables<I>.EndPeriod()
        {
            return this;
        }

        IDefineGroup<I> IEndGroupOrDefinePeriod<I>.EndGroup()
        {
            return this;
        }

        #endregion

        #region Groups

        private GroupOfVariables _currentGroup;
        private readonly IList<GroupOfVariables> _Groups = new List<GroupOfVariables>();

        public IDefinePeriod<I> BeginGroup(string caption, Func<object, IEnumerable> members)
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

        public IDefinePeriod<I> BeginGroup(string caption, Func<IEnumerable> members)
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
        private IList<MetaVariable<H, I>> _variables = new List<MetaVariable<H, I>>();

        /// <summary>
        /// Объявляет простую переменную
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metaVar"></param>
        /// <returns></returns>
        IPeriodVariableAssigment<I> IPeriodVariables<I>.Let<T>(Expression<Func<T>> metaVar)
        {            
            return (this as IPeriodVariables<I>).Let(string.Empty, metaVar);
        }

        IPeriodVariableAssigment<I> IPeriodVariables<I>.Let<T>(string name, Expression<Func<T>> metaVar)
        {
            var me = metaVar.Body as MemberExpression;
            _currentPropertyInfo = me.Member as PropertyInfo;
            _currentVariableName = string.IsNullOrEmpty(name) ? _currentPropertyInfo.Name : name;

            return this;
        }

        #endregion

        #region Sliced variable declaring

        IPeriodVariableAssigment<I> IPeriodVariables<I>.Let<T>(Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn)
        {
            return (this as IPeriodVariables<I>).Let(string.Empty, metaVar, basedOn);
        }

        /// <summary>
        /// Объявляет переменную в виде списка, занося в нее значения из другой переменной за соответсвующий период
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="metaVar"></param>
        /// <param name="basedOn"></param>
        /// <returns></returns>
        IPeriodVariableAssigment<I> IPeriodVariables<I>.Let<T>(string name, Expression<Func<IList<T>>> metaVar, Expression<Func<T>> basedOn)
        {
            (this as IPeriodVariables<I>).Let(name, metaVar);

            MemberExpression me = basedOn.Body as MemberExpression;
            PropertyInfo baseProperty = me.Member as PropertyInfo;

            _variables.Add(new SlicedVariable<H, T, I>
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

        IPeriodVariableAssigment<I> IPeriodVariables<I>.Let<K, T>(Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn)
        {            
            return (this as IPeriodVariables<I>).Let(String.Empty, metaVar, basedOn);
        }

        IPeriodVariableAssigment<I> IPeriodVariables<I>.Let<K, T>(string name, Expression<Func<IDictionary<K, T>>> metaVar, Expression<Func<T>> basedOn)
        {
            (this as IPeriodVariables<I>).Let(name, metaVar);

            MemberExpression me = basedOn.Body as MemberExpression;
            PropertyInfo baseProperty = me.Member as PropertyInfo;

            _variables.Add(new CrossGroupVariable<H, K, T, I>
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

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<T>> basedOn, int shift)
        {
            Func<TimeArg<I>, T> wrap = tp => default(T);
            return (this as IPeriodVariableAssigment<I>).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<T> emptyFiller)
        {
            Func<TimeArg<I>, T> wrap = tp => emptyFiller();
            return (this as IPeriodVariableAssigment<I>).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<I, T> emptyFiller)
        {
            Func<TimeArg<I>, T> wrap = tp => emptyFiller(tp.T);
            return (this as IPeriodVariableAssigment<I>).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<I, Int32, T> emptyFiller)
        {
            Func<TimeArg<I>, T> wrap = tp => emptyFiller(tp.T, tp.I);
            return (this as IPeriodVariableAssigment<I>).Assign(basedOn, shift, wrap);
        }

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<T>> basedOn, int shift, Func<TimeArg<I>, T> emptyFiller)
        {
            MemberExpression me = basedOn.Body as MemberExpression;
            PropertyInfo baseProperty = me.Member as PropertyInfo;

            var variable = new ShiftedVaraible<H, T, I>
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

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<T>> writer)
        {
            return (this as IPeriodVariableAssigment<I>).Assign<T>(ExpressionsBuilder<I>.ConvertExpression<T>(writer));
        }

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<I, T>> writer)
        {
            return (this as IPeriodVariableAssigment<I>).Assign<T>(ExpressionsBuilder<I>.ConvertExpression<T>(writer));
        }

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<I, int, T>> writer)
        {
            return (this as IPeriodVariableAssigment<I>).Assign<T>(ExpressionsBuilder<I>.ConvertExpression<T>(writer));
        }

        IPeriodVariableReader<I> IPeriodVariableAssigment<I>.Assign<T>(Expression<Func<TimeArg<I>, T>> writer)
        {
            var variable = new CalculatedVariable<H, T, I>
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

        IPeriodVariableReader<I> IPeriodVariableReader<I>.Read(Action reader)
        {
            return (this as IPeriodVariableReader<I>).Read((TimeArg<I> tp) => reader());
        }

        IPeriodVariableReader<I> IPeriodVariableReader<I>.Read(Action<I> reader)
        {
            return (this as IPeriodVariableReader<I>).Read(tp => reader(tp.T));
        }

        IPeriodVariableReader<I> IPeriodVariableReader<I>.Read(Action<I, int> reader)
        {
            return (this as IPeriodVariableReader<I>).Read(tp => reader(tp.T, tp.I));
        }

        IPeriodVariableReader<I> IPeriodVariableReader<I>.Read(Action<TimeArg<I>> reader)
        {
            _variables.Last().Readers.Add(reader);
            return this;
        }

        IPeriodVariables<I> IPeriodVariableReader<I>.End()
        {
            return this;
        }

        #endregion

        #region Summary

        IEndGroupOrDefinePeriod<I> IPeriodVariables<I>.Summarize(Action<I> action)
        {
            return (this as IPeriodVariables<I>).Summarize(tp => action(tp.T));
        }

        IEndGroupOrDefinePeriod<I> IPeriodVariables<I>.Summarize(Action<I, Int32> action)
        {
            return (this as IPeriodVariables<I>).Summarize(tp => action(tp.T, tp.I));
        }

        IEndGroupOrDefinePeriod<I> IPeriodVariables<I>.Summarize(Action<TimeArg<I>> action)
        {
            _variables.Add(new SummarizeVariable<H, I>(action)
            {
                Name = $"summary for {_currentPeriod.Name ?? "unnamed"} period",
                Period = _currentPeriod,
                Group = _currentGroup,
            });

            return this;
        }

        #endregion

        #region Compilation

        public static IComputable<I> Compile(SeriesConnector<H, I> source)
        {
            foreach (MetaVariable<H, I> v in source._variables)
            {
                v.Compile(source._variables);
            }
            return source;
        }

        #endregion

        #region Execution

        void IComputable<I>.Compute(ComputationParameters<I> parameters)
        {
            IEnumerable<MetaVariable<H, I>> varsToCompute;
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

        event EventHandler<ProgressArgs> IComputable<I>.OnProgress
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

        event EventHandler<ComputationArgs> IComputable<I>.OnComputationStart
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

        event EventHandler<ComputationArgs> IComputable<I>.OnComputationFinish
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

        event EventHandler<ComputationErrorArgs> IComputable<I>.OnComputationError
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

        private void MarkVariablesToCompute(IEnumerable<MetaVariable<H, I>> input)
        {
            foreach (var v in input)
            {
                v.RequiredCalculation = true;
                MarkVariablesToCompute(v.DependsOn);
            }
        }

        private IEnumerable<MetaVariable<H, I>> MarkedVariables
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

                        foreach (MetaVariable<H, I> v in variablesInGroup)
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
