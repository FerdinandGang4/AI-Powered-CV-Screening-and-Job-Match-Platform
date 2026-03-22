# Class Diagram Relationships

This document explains the class relationships for the AI-Powered CV Screening and Job Match Platform.

## Core Classes

- `Recruiter`
  Represents the system user who creates job postings and reviews ranked candidates.

- `JobPosting`
  Represents a job opportunity uploaded into the platform. It contains the role description and overall requirements for candidate matching.

- `JobRequirement`
  Represents a single requirement attached to a job posting, such as a skill, years of experience, education, or certification.

- `Candidate`
  Represents an applicant whose CV is uploaded and analyzed by the system.

- `CvDocument`
  Represents the uploaded CV file and the parsed raw text extracted from it.

- `CandidateSkill`
  Represents an individual skill found in the candidate CV, including proficiency and years of use.

- `WorkExperience`
  Represents a candidate's employment history and work background.

- `CandidateProject`
  Represents important projects completed by the candidate and their technologies or business domain.

- `EducationRecord`
  Represents the candidate's academic background.

- `CandidateEvaluation`
  Represents the result of matching one candidate against one job posting. It stores scores and the final recommendation.

- `SkillGap`
  Represents a missing or weak requirement identified during evaluation.

- `MatchExplanation`
  Represents the human-readable explanation for why the candidate matched well or poorly.

- `RankingReport`
  Represents the ranked results for all candidates evaluated against a specific job posting.

## Relationships Between Classes

- `Recruiter` to `JobPosting`
  One recruiter can create many job postings.

- `JobPosting` to `JobRequirement`
  One job posting contains one or more job requirements.

- `Candidate` to `CvDocument`
  One candidate owns one CV document in the first version of the system.

- `Candidate` to `CandidateSkill`
  One candidate can have many extracted skills.

- `Candidate` to `WorkExperience`
  One candidate can have many work experience records.

- `Candidate` to `CandidateProject`
  One candidate can have many projects.

- `Candidate` to `EducationRecord`
  One candidate can have many education records.

- `CandidateEvaluation` to `Candidate`
  Each evaluation belongs to exactly one candidate.

- `CandidateEvaluation` to `JobPosting`
  Each evaluation is performed for exactly one job posting.

- `JobPosting` to `CandidateEvaluation`
  One job posting can have many candidate evaluations.

- `Candidate` to `CandidateEvaluation`
  One candidate can appear in many evaluations if they are compared with multiple job postings.

- `CandidateEvaluation` to `SkillGap`
  One evaluation can identify many missing or weak skills.

- `CandidateEvaluation` to `MatchExplanation`
  Each evaluation produces one explanation summary for recruiters.

- `RankingReport` to `CandidateEvaluation`
  One ranking report contains many candidate evaluations ordered by score.

- `RankingReport` to `JobPosting`
  Each ranking report is generated for one job posting.

## Relationship Summary

- `Recruiter` creates `JobPosting`
- `JobPosting` defines `JobRequirement`
- `Candidate` owns `CvDocument`
- `Candidate` contains `CandidateSkill`, `WorkExperience`, `CandidateProject`, and `EducationRecord`
- `CandidateEvaluation` links `Candidate` and `JobPosting`
- `CandidateEvaluation` produces `MatchExplanation` and identifies `SkillGap`
- `RankingReport` ranks multiple `CandidateEvaluation` results for one `JobPosting`

## Design Notes

- `CandidateEvaluation` is the central business class because it connects candidates, job requirements, scores, skill gaps, and explanations.
- `RankingReport` is a reporting aggregate built after evaluations are completed.
- `JobRequirement` is separated from `JobPosting` so the matching engine can score requirements individually with different weights.
- `SkillGap` and `MatchExplanation` are separated from the candidate entity because they belong to a specific evaluation result, not the candidate in general.
