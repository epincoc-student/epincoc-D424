using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.Classes.Models;
using D424__Eric_Pincock.Classes.Models.Majors;
using D424__Eric_Pincock.Classes.Models.UserCourseDetails;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock
{
    public class LocalDbService
    {
        private const string DB_NAME = "local_db.db3";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            _connection = new SQLiteAsyncConnection(
                Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
        }

        public async Task InitAsync()
        {
            _connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
            await _connection.CreateTableAsync<DegreeRecord>();
            await _connection.CreateTableAsync<Course>();
            await _connection.CreateTableAsync<DegreeCourses>();
        }

        public async Task UserTableCreation()
        {
            await _connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
            await _connection.CreateTableAsync<UserTerm>(); 
            await _connection.CreateTableAsync<UserDegree>();
            await _connection.CreateTableAsync<UserDegreeCourse>();
            await _connection.CreateTableAsync<CourseInstructor>();
            await _connection.CreateTableAsync<PerformanceAssessment>();
            await _connection.CreateTableAsync<ObjectiveAssessment>();

        }

        public async Task ClearDatabaseAsync()
        {
            await _connection.DeleteAllAsync<DegreeCourses>();
            await _connection.DeleteAllAsync<Course>();
            await _connection.DeleteAllAsync<DegreeRecord>();
        }

        public async Task SeedDegreesFromCatalogAsync()
        {
            var existing = await _connection.Table<DegreeRecord>().ToListAsync();
            if (existing.Any()) return;

            foreach (var degree in DegreeCatalog.AllDegrees)
            {
                var record = new DegreeRecord
                {
                    DegreeId = degree.DegreeId,
                    DegreeName = degree.DegreeName,
                    DegreeType = degree.DegreeType
                };

                await _connection.InsertAsync(record);

                foreach (var dc in degree.DegreeCourses)
                {
                    dc.DegreeId = degree.DegreeId;
                    await _connection.InsertAsync(dc);
                }

                Console.WriteLine($"Seeding {DegreeCatalog.AllDegrees.Count} degrees...");
            }
        }

        public async Task SaveOrUpdateUserDegreeAsync(UserDegree userDegree)
        {
            var existing = await GetUserDegreeByUserIdAsync(userDegree.UserId);
            if (existing == null)
            {
                await _connection.InsertAsync(userDegree);
            }
            else
            {
                existing.DegreeId = userDegree.DegreeId;
                await _connection.UpdateAsync(existing);
            }
        }
        public async Task<UserDegree> GetUserDegreeByUserIdAsync(int userId)
        {
            return await _connection.Table<UserDegree>()
                                   .Where(ud => ud.UserId == userId)
                                   .FirstOrDefaultAsync();
        }

        // User CRUD
        public async Task<User> GetUserByUserId(int  userId)
        {
            return await _connection.Table<User>()
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _connection.Table<User>()
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _connection.DeleteAsync<User>(userId);

            var userDegrees = await _connection.Table<UserDegree>()
                .Where(ud => ud.UserId == userId).ToListAsync();

            foreach (var degree in userDegrees)
            {
                await _connection.DeleteAsync(degree);
            }

            var userCourses = await _connection.Table<UserDegreeCourse>()
                .Where(uc => uc.UserId == userId).ToListAsync();

            foreach (var course in userCourses)
            {
                await _connection.DeleteAsync(course);
            }
        }

        // User Terms CRUD
        public async Task InsertUserTermAsync(UserTerm term)
        {
            await _connection.InsertAsync(term);
        }

        public async Task<List<UserTerm>> GetUserTermsAsync(int userId)
        {
            return await _connection.Table<UserTerm>()
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.TermStartDate)
                .ToListAsync();
        }

        public async Task<Course> GetCourseById(int courseId)
        {
            return await _connection.Table<Course>()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<UserTerm> GetTermByIdAsync(int termId)
        {
            return await _connection.Table<UserTerm>()
                .FirstOrDefaultAsync(t => t.TermId == termId);
        }

        public async Task UpdateUserTermAsync(UserTerm term)
        {
            await _connection.UpdateAsync(term);
        }

        public async Task DeleteUserTermAsync(int termId)
        {
            var term = await GetTermByIdAsync(termId);
            if (term != null)
            {
                await _connection.DeleteAsync(term);
            }
        }

        public async Task AssignCourseToTermAsync(int courseId, int termId)
        {
            var course = await _connection.Table<UserDegreeCourse>()
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course != null)
            {
                course.TermId = termId;
                await _connection.UpdateAsync(course);
            }
        }

        // Pull list of courses based on their associated term Id
        public async Task<List<UserDegreeCourse>> GetCoursesByTermAsync(int userId, int termId)
        {
            return await _connection.Table<UserDegreeCourse>()
                .Where(c => c.UserId == userId && c.TermId == termId)
                .ToListAsync();
        }

        //User DegreeCourse CRUD
        public async Task InsertUserDegreeCourseAsync(UserDegreeCourse userCourse)
        {
            await _connection.InsertAsync(userCourse);
        }

        public async Task UpdateUserDegreeCourseAsync(UserDegreeCourse updatedCourse, int termId)
        {
            var existingCourse = await _connection.Table<UserDegreeCourse>()
                .FirstOrDefaultAsync(uc => uc.Id == updatedCourse.Id);

            if (existingCourse != null)
            {
                existingCourse.DegreeId = updatedCourse.DegreeId; 
                existingCourse.TermId = termId;
                existingCourse.StartDate = updatedCourse.StartDate;
                existingCourse.EndDate = updatedCourse.EndDate;
                existingCourse.Status = updatedCourse.Status;
                existingCourse.Notes = updatedCourse.Notes;

                await _connection.UpdateAsync(existingCourse);
            }
        }
        public async Task<UserDegreeCourse> GetUserDegreeCourseByUserAndCourseId(int userId, int courseId)
        {
            return await _connection.Table<UserDegreeCourse>()
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);
        }


        public async Task<List<UserDegreeCourse>> GetUserDegreeCoursesAsync(int userId, int degreeId)
        {
            return await _connection.Table<UserDegreeCourse>()
                .Where(c => c.UserId == userId && c.DegreeId == degreeId)
                .ToListAsync();
        }

        public async Task<bool> UserDegreeCourseExistsAsync(int userId, int degreeId, int courseId)
        {
            var match = await _connection.Table<UserDegreeCourse>()
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.DegreeId == degreeId && uc.CourseId == courseId);

            return match != null;
        }

        public async Task<Degree> GetDegreeWithCoursesAsync(DegreeRecord record)
        {
            // Get the matching subclass from DegreeCatalog
            var degree = DegreeCatalog.GetById(record.DegreeId);
            if (degree == null) return null;

            // Load associated courses from the database
            var links = await _connection.Table<DegreeCourses>()
                .Where(dc => dc.DegreeId == record.DegreeId)
                .ToListAsync();

            foreach (var link in links)
            {
                link.Course = await _connection.Table<Course>()
                    .Where(c => c.CourseId == link.CourseId)
                    .FirstOrDefaultAsync();

                if (link.Course?.PreReqsNeeded == true)
                {
                    link.Course.RequiredCourses = await _connection.Table<Course>()
                        .Where(c => c.CourseId == link.Course.PreReqCourses)
                        .FirstOrDefaultAsync();
                }
            }

            degree.DegreeCourses = links;
            return degree;
        }

        public async Task<List<DegreeRecord>> GetAllDegreesAsync()
        {
            return await _connection.Table<DegreeRecord>().ToListAsync();
        }

        //UserDegreeCourse Details

        public async Task<CourseInstructor> GetInstructorByCourseIdAsync(int courseId)
        {
            return await _connection.Table<CourseInstructor>()
                .FirstOrDefaultAsync(ci => ci.CourseId == courseId);
        }

        public async Task<List<PerformanceAssessment>> GetPerformanceAssessmentsByCourseIdAsync(int courseId)
        {
            return await _connection.Table<PerformanceAssessment>()
                .Where(pa => pa.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<ObjectiveAssessment?> GetObjectiveAssessmentByCourseIdAsync(int courseId)
        {
            return await _connection.Table<ObjectiveAssessment>()
                .Where(oa => oa.CourseId == courseId)
                .FirstOrDefaultAsync();
        }

        public async Task<PerformanceAssessment?> GetPerformanceAssessmentByCourseIdAsync(int courseId)
        {
            return await _connection.Table<PerformanceAssessment>()
                .Where(pa => pa.CourseId == courseId)
                .FirstOrDefaultAsync();
        }


        // Course Instructor CRUD
        public async Task SaveOrUpdateCourseInstructorAsync(CourseInstructor instructor)
        {
            var existing = await GetInstructorByCourseIdAsync(instructor.CourseId);

            if (existing == null)
            {
                await _connection.InsertAsync(instructor);
            }
            else
            {
                instructor.CourseInstructorId = existing.CourseInstructorId; 
                await _connection.UpdateAsync(instructor);
            }
        }
        public async Task DeleteCourseInstructorByIdAsync(int courseInstructorId)
        {
            await _connection.DeleteAsync<CourseInstructor>(courseInstructorId);
        }

        public async Task<CourseInstructor> GetCourseInstructorByIdAsync(int courseInstructorId)
        {
            return await _connection.FindAsync<CourseInstructor>(courseInstructorId);
        }

        //Objective Assessment CRUD
        public async Task SaveOrUpdateObjectiveAssessmentAsync(ObjectiveAssessment assessment)
        {
            var existing = await GetObjectiveAssessmentByCourseIdAsync(assessment.CourseId);

            if (existing == null)
            {
                await _connection.InsertAsync(assessment);
            }
            else
            {
                assessment.ObjectiveAssessmentId = existing.ObjectiveAssessmentId;
                await _connection.UpdateAsync(assessment);
            }
        }

        public async Task DeleteObjectiveAssessmentById(int objectiveAssessmentId)
        {
            await _connection.DeleteAsync<ObjectiveAssessment>(objectiveAssessmentId);
        }

        public async Task<ObjectiveAssessment> GetObjectiveAssessmentById(int objectiveAssessmentId)
        {
            return await _connection.FindAsync<ObjectiveAssessment>(objectiveAssessmentId);
        }

        //Performance Assessment CRUD
        public async Task SaveOrUpdatePerformanceAssessmentAsync(PerformanceAssessment assessment)
        {
            var existing = await GetPerformanceAssessmentByCourseIdAsync(assessment.CourseId);

            if (existing == null)
            {
                await _connection.InsertAsync(assessment);
            }
            else
            {
                assessment.PerformanceAssessmentId = existing.PerformanceAssessmentId;
                await _connection.UpdateAsync(assessment);
            }
        }

        public async Task DeletePerformanceAssessmentById(int performanceAssessmentId)
        {
            await _connection.DeleteAsync<PerformanceAssessment>(performanceAssessmentId);
        }

        public async Task<PerformanceAssessment> GetPerformanceAssessmentById(int performanceAssessmentId)
        {
            return await _connection.FindAsync<PerformanceAssessment>(performanceAssessmentId);
        }



        // Seed User Table for testing
        public async Task SeedUsersAsync()
        {
            await _connection.CreateTableAsync<User>();

            var existingUsers = await _connection.Table<User>().ToListAsync();
            if (existingUsers.Count > 0)
                return; // Skip seeding if users already exist

            var salt1 = SecurityHelper.GenerateSalt();
            var salt2 = SecurityHelper.GenerateSalt();
            var salt3 = SecurityHelper.GenerateSalt();


            var user1 = new User
            {
                UserName = "aanderson",
                UserFirstName = "Alice",
                UserLastName = "Anderson",
                PasswordSalt = salt1,
                PasswordHash = SecurityHelper.HashPassword("password123", salt1)
            };

            var user2 = new User
            {
                UserName = "bbrown",
                UserFirstName = "Bob",
                UserLastName = "Brown",
                PasswordSalt = salt2,
                PasswordHash = SecurityHelper.HashPassword("secure456", salt2)
            };

            var user3 = new User
            {
                UserName = "ccox",
                UserFirstName = "Charlie",
                UserLastName = "Cox",
                PasswordSalt = salt3,
                PasswordHash = SecurityHelper.HashPassword("lollipop123", salt3)
            };


            await _connection.InsertAsync(user1);
            await _connection.InsertAsync(user2);
            await _connection.InsertAsync(user3);

        }

        // Populates the courses table
        public async Task SeedCoursesAsync()
        {
            var existing = await _connection.Table<Course>().ToListAsync();
            if (existing.Any()) return;

            var courses = new List<Course>
    {
        // Computer Science
        new Course { CourseId = 101, CourseName = "Intro to Programming", CourseCredits = 3 },
        new Course { CourseId = 102, CourseName = "Data Structures", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 101 },
        new Course { CourseId = 103, CourseName = "Algorithms", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 102 },
        new Course { CourseId = 104, CourseName = "Operating Systems", CourseCredits = 4 },
        new Course { CourseId = 105, CourseName = "Software Engineering", CourseCredits = 3 },
        new Course { CourseId = 106, CourseName = "Machine Learning", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 103 },
        new Course { CourseId = 107, CourseName = "Web Development", CourseCredits = 3 },
        new Course { CourseId = 108, CourseName = "Mobile App Development", CourseCredits = 3 },
        new Course { CourseId = 109, CourseName = "Database Systems", CourseCredits = 3 },
        new Course { CourseId = 110, CourseName = "Computer Architecture", CourseCredits = 3 },

        // Statistics & Math
        new Course { CourseId = 201, CourseName = "Statistics I", CourseCredits = 3 },
        new Course { CourseId = 202, CourseName = "Probability Theory", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 201 },
        new Course { CourseId = 203, CourseName = "Statistical Inference", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 202 },
        new Course { CourseId = 204, CourseName = "Linear Algebra", CourseCredits = 3 },
        new Course { CourseId = 205, CourseName = "Calculus I", CourseCredits = 4 },
        new Course { CourseId = 206, CourseName = "Calculus II", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 205 },
        new Course { CourseId = 207, CourseName = "Numerical Methods", CourseCredits = 3 },
        new Course { CourseId = 208, CourseName = "Discrete Mathematics", CourseCredits = 3 },
        new Course { CourseId = 209, CourseName = "Data Visualization", CourseCredits = 3 },
        new Course { CourseId = 210, CourseName = "Mathematical Modeling", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 206 },

        // Business & Economics
        new Course { CourseId = 301, CourseName = "Principles of Economics", CourseCredits = 3 },
        new Course { CourseId = 302, CourseName = "Microeconomics", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 301 },
        new Course { CourseId = 303, CourseName = "Macroeconomics", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 302 },
        new Course { CourseId = 304, CourseName = "Business Statistics", CourseCredits = 3 },
        new Course { CourseId = 305, CourseName = "Financial Accounting", CourseCredits = 3 },
        new Course { CourseId = 306, CourseName = "Managerial Accounting", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 305 },
        new Course { CourseId = 307, CourseName = "Marketing Principles", CourseCredits = 3 },
        new Course { CourseId = 308, CourseName = "Organizational Behavior", CourseCredits = 3 },
        new Course { CourseId = 309, CourseName = "Business Ethics", CourseCredits = 3 },
        new Course { CourseId = 310, CourseName = "Strategic Management", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 308 },

        // Psychology & Humanities
        new Course { CourseId = 401, CourseName = "Introduction to Psychology", CourseCredits = 3 },
        new Course { CourseId = 402, CourseName = "Developmental Psychology", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 401 },
        new Course { CourseId = 403, CourseName = "Social Psychology", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 402 },
        new Course { CourseId = 404, CourseName = "Introduction to Sociology", CourseCredits = 3 },
        new Course { CourseId = 405, CourseName = "Cultural Anthropology", CourseCredits = 3 },
        new Course { CourseId = 406, CourseName = "Ethics and Society", CourseCredits = 3 },
        new Course { CourseId = 407, CourseName = "Philosophy of Mind", CourseCredits = 3 },
        new Course { CourseId = 408, CourseName = "World History", CourseCredits = 3 },
        new Course { CourseId = 409, CourseName = "American Literature", CourseCredits = 3 },
        new Course { CourseId = 410, CourseName = "Creative Writing", CourseCredits = 3 },

        // Science
        new Course { CourseId = 501, CourseName = "General Biology", CourseCredits = 4 },
        new Course { CourseId = 502, CourseName = "Genetics", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 501 },
        new Course { CourseId = 503, CourseName = "Cell Biology", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 502 },
        new Course { CourseId = 504, CourseName = "General Chemistry", CourseCredits = 4 },
        new Course { CourseId = 505, CourseName = "Organic Chemistry", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 504 },
        new Course { CourseId = 506, CourseName = "Biochemistry", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 505 },
        new Course { CourseId = 507, CourseName = "Physics I", CourseCredits = 4 },
        new Course { CourseId = 508, CourseName = "Physics II", CourseCredits = 4, PreReqsNeeded = true, PreReqCourses = 507 },
        new Course { CourseId = 509, CourseName = "Environmental Science", CourseCredits = 3 },
        new Course { CourseId = 510, CourseName = "Scientific Research Methods", CourseCredits = 3 },

        // General Education
        new Course { CourseId = 601, CourseName = "Introduction to Philosophy", CourseCredits = 3 },
        new Course { CourseId = 602, CourseName = "Ethics and Moral Reasoning", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 601 },
        new Course { CourseId = 603, CourseName = "Philosophy of Science", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 602 },
        new Course { CourseId = 604, CourseName = "Art Appreciation", CourseCredits = 3 },
        new Course { CourseId = 605, CourseName = "History of Western Art", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 604 },
        new Course { CourseId = 606, CourseName = "Creative Writing", CourseCredits = 3 },
        new Course { CourseId = 607, CourseName = "Advanced Creative Writing", CourseCredits = 3, PreReqsNeeded = true, PreReqCourses = 606 },
        new Course { CourseId = 608, CourseName = "World History", CourseCredits = 3 },
        new Course { CourseId = 609, CourseName = "U.S. History Since 1877", CourseCredits = 3 },
        new Course { CourseId = 610, CourseName = "Comparative World Religions", CourseCredits = 3 }
    };

            foreach (var course in courses)
                await _connection.InsertAsync(course);
        }
    }
}