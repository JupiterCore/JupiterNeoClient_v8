using JupiterNeoServiceClient.classes;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Models
{

    public class SModel
    {
        public string? basc_id { get; set; }
        public string? basc_date { get; set; }
        public string? basc_time { get; set; }
        public int basc_started { get; set; }
        public int basc_completed { get; set; }
        public int basc_scanned { get; set; }

    }

    public class SchedulesModel : BaseModel
    {

        public enum fd { ID, DATE, TIME, STARTED, COMPLETED };

        public SchedulesModel()
        {
            this.TableName = "backup_schedule";
            this.fields = new Dictionary<Enum, string>();

            this.fields[fd.ID] = "basc_id";
            this.fields[fd.DATE] = "basc_date";
            this.fields[fd.TIME] = "basc_time";
            this.fields[fd.STARTED] = "basc_started";
            this.fields[fd.COMPLETED] = "basc_completed";

        }

        public void deleteCompletedBackups()
        {

            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }

            string today = Helpers.today();
            this.query()
                .Where(fields[fd.DATE], "<>", today)
                .WhereNotNull(fields[fd.COMPLETED])
                .Delete();
        }

        public SModel? getSchedule(string time)
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            string today = Helpers.today();
            return this.query().Where(fields[fd.DATE], today).Where(fields[fd.TIME], time).Get<SModel>().FirstOrDefault<SModel>();
        }

        public SModel? getSchedue(string scheduleId)
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            return this.query().Where(fields[fd.ID], scheduleId).Get<SModel>().FirstOrDefault();
        }

        public SModel? getUncompletedBackup()
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            return this.query()
                    .Where(fields[fd.STARTED], 1)
                    .Where(fields[fd.COMPLETED], 0)
                    .Get<SModel>().FirstOrDefault();
        }


        /**
         * Solo comenzar un backup no comenzado que es del día de hoy.
         */
        public SModel? getUnstartedBackup()
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            string today = Helpers.today();
            return this.query()
                    .Where(fields[fd.STARTED], 0)
                    .Where(fields[fd.COMPLETED], 0)
                    .Where(fields[fd.DATE], today)
                    .Get<SModel>()
                    .FirstOrDefault();
        }

        public IEnumerable<SModel> getAllUnstartedBackups()
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            string today = Helpers.today();
            return this.query()
                    .Where(fields[fd.STARTED], 0)
                    .Where(fields[fd.COMPLETED], 0)
                    .Where(fields[fd.DATE], today)
                    .Get<SModel>();
        }

        public void markScheduleAsStarted(SModel m)
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            m.basc_started = 1;
            this.query().Where(fields[fd.ID], m.basc_id)
                .Update(m);
        }

        public void markScheduleAsCompleted(SModel m)
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            m.basc_completed = 1;
            this.query()
                .Where(fields[fd.ID], m.basc_id)
                .Update(m);
        }

        public int insertSchedule(string time)
        {
            SModel model = new SModel();
            model.basc_date = Helpers.today();
            model.basc_time = time;
            model.basc_started = 0;
            model.basc_completed = 0;
            model.basc_scanned = 0;
            return this.insert(model);
        }

        public void markScheduleAsScanned(SModel schedule)
        {
            if (this.fields == null)
            {
                throw new Exception("[] fields is null");
            }
            schedule.basc_scanned = 1;
            this.query()
                .Where(fields[fd.ID], schedule.basc_id)
                .Update(schedule);
        }

    }

}
